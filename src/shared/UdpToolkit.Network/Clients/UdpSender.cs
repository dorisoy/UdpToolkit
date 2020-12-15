namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Pooling;

    public sealed class UdpSender : IUdpSender
    {
        private const int MtuSizeLimit = 1500;

        private readonly UdpClient _sender;

        private readonly IRawRoomManager _rawRoomManager;
        private readonly IObjectsPool<NetworkPacket> _networkPacketPool;

        private readonly ILogger _logger = Log.Logger.ForContext<UdpSender>();

        public UdpSender(
            UdpClient sender,
            IRawRoomManager rawRoomManager,
            IObjectsPool<NetworkPacket> networkPacketPool)
        {
            _sender = sender;
            _rawRoomManager = rawRoomManager;
            _networkPacketPool = networkPacketPool;
            _logger.Debug($"{nameof(UdpSender)} - {sender.Client.LocalEndPoint} created");
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        public Task SendAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            return SendInternalAsync(pooledNetworkPacket);
        }

        public Task SendAsync(
            int roomId,
            PooledObject<NetworkPacket> pooledNetworkPacket,
            BroadcastType broadcastType)
        {
            switch (broadcastType)
            {
                case BroadcastType.Room:
                    return HandleRoom(roomId, pooledNetworkPacket);
                case BroadcastType.RoomExceptCaller:
                    return HandleRoomExceptCaller(roomId, pooledNetworkPacket);
                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastType), broadcastType, null);
            }
        }

        public Task SendAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IRawPeer rawPeer,
            BroadcastType broadcastType)
        {
            switch (broadcastType)
            {
                case BroadcastType.Caller:
                    return HandleCaller(pooledNetworkPacket, rawPeer);
                case BroadcastType.Server:
                    return HandleServer(pooledNetworkPacket, rawPeer);
                case BroadcastType.AckToServer:
                    return HandleAckToServer(pooledNetworkPacket, rawPeer);
                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastType), broadcastType, null);
            }
        }

        private async Task SendInternalAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var bytes = NetworkPacket.Serialize(pooledNetworkPacket.Value);

            if (bytes.Length > MtuSizeLimit)
            {
                _logger.Error($"Udp packet oversize mtu limit - {bytes.Length}");

                return;
            }

            _logger.Debug($"Packet from - {_sender.Client.LocalEndPoint} to {pooledNetworkPacket.Value.IpEndPoint} sended");
            _logger.Debug(
                messageTemplate: "Packet sends: {@packet}, Total bytes length: {@length}, Payload bytes length: {@payload}",
                propertyValue0: pooledNetworkPacket.Value,
                propertyValue1: bytes.Length,
                propertyValue2: pooledNetworkPacket.Value.Serializer().Length);

            await _sender
                .SendAsync(bytes, bytes.Length, pooledNetworkPacket.Value.IpEndPoint)
                .ConfigureAwait(false);
        }

        private Task HandleCaller(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IRawPeer rawPeer)
        {
            return Send(rawPeer, pooledNetworkPacket);
        }

        private Task HandleRoom(
            int roomId,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            return _rawRoomManager
                .Apply(
                    caller: pooledNetworkPacket.Value.PeerId,
                    roomId: roomId,
                    condition: (peer) => true,
                    func: (peer) => Send(peer, pooledNetworkPacket));
        }

        private Task HandleServer(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IRawPeer rawPeer)
        {
            rawPeer
                .GetOutcomingChannel(pooledNetworkPacket.Value.ChannelType)
                .HandleOutputPacket(pooledNetworkPacket.Value);

            return SendInternalAsync(pooledNetworkPacket);
        }

        private Task HandleRoomExceptCaller(
            int roomId,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            return _rawRoomManager
                .Apply(
                    caller: pooledNetworkPacket.Value.PeerId,
                    roomId: roomId,
                    condition: (peer) => peer.PeerId != pooledNetworkPacket.Value.PeerId,
                    func: (peer) => Send(peer, pooledNetworkPacket));
        }

        private Task HandleAckToServer(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IRawPeer rawPeer)
        {
            rawPeer
                .GetOutcomingChannel(pooledNetworkPacket.Value.ChannelType)
                .GetAck(pooledNetworkPacket.Value);

            pooledNetworkPacket.Value.Set(ipEndPoint: rawPeer.GetRandomIp());

            return SendInternalAsync(pooledNetworkPacket);
        }

        private Task Send(
            IRawPeer peer,
            PooledObject<NetworkPacket> originalPooledNetworkPacket)
        {
            if (peer.PeerId == originalPooledNetworkPacket.Value.PeerId && originalPooledNetworkPacket.Value.IsReliable)
            {
                // produce ack
                var pooledNetworkPacket = _networkPacketPool.Get();

                originalPooledNetworkPacket.Value.Set(networkPacketType: NetworkPacketType.Ack);

                pooledNetworkPacket.Value.Set(
                    hookId: originalPooledNetworkPacket.Value.HookId,
                    channelType: originalPooledNetworkPacket.Value.ChannelType,
                    networkPacketType: NetworkPacketType.Ack,
                    peerId: originalPooledNetworkPacket.Value.PeerId,
                    id: originalPooledNetworkPacket.Value.Id,
                    acks: originalPooledNetworkPacket.Value.Acks,
                    serializer: originalPooledNetworkPacket.Value.Serializer,
                    createdAt: originalPooledNetworkPacket.Value.CreatedAt,
                    ipEndPoint: peer.GetRandomIp());

                peer
                    .GetOutcomingChannel(pooledNetworkPacket.Value.ChannelType)
                    .GetAck(pooledNetworkPacket.Value);

                return SendInternalAsync(pooledNetworkPacket);
            }
            else
            {
                // produce packet
                var pooledNetworkPacket = _networkPacketPool.Get();

                pooledNetworkPacket.Value.Set(
                    id: originalPooledNetworkPacket.Value.Id,
                    acks: originalPooledNetworkPacket.Value.Acks,
                    hookId: originalPooledNetworkPacket.Value.HookId,
                    ipEndPoint: peer.GetRandomIp(),
                    createdAt: DateTimeOffset.UtcNow,
                    serializer: originalPooledNetworkPacket.Value.Serializer,
                    channelType: originalPooledNetworkPacket.Value.ChannelType,
                    peerId: peer.PeerId,
                    networkPacketType: NetworkPacketType.FromServer);

                peer
                    .GetOutcomingChannel(channelType: pooledNetworkPacket.Value.ChannelType)
                    .HandleOutputPacket(pooledNetworkPacket.Value);

                return SendInternalAsync(pooledNetworkPacket);
            }
        }
    }
}
