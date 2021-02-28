namespace UdpToolkit.Jobs
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class SenderJob
    {
        private readonly IAsyncQueue<CallContext> _outputQueue;
        private readonly IConnectionPool _connectionPool;
        private readonly RoomManager _roomManager;

        public SenderJob(
            IAsyncQueue<CallContext> outputQueue,
            IConnectionPool connectionPool,
            RoomManager roomManager)
        {
            _outputQueue = outputQueue;
            _connectionPool = connectionPool;
            _roomManager = roomManager;
        }

        public async Task Execute(
            IUdpSender udpSender)
        {
            foreach (var callContext in _outputQueue.Consume())
            {
                var networkPacket = callContext.NetworkPacket;
                var connectionId = callContext.NetworkPacket.ConnectionId;

                await ExecuteInternal(
                            udpSender: udpSender,
                            roomId: callContext.RoomId,
                            broadcastMode: callContext.BroadcastMode,
                            networkPacket: networkPacket)
                        .ConfigureAwait(false);
            }
        }

        private async Task ExecuteInternal(
            IUdpSender udpSender,
            int? roomId,
            BroadcastMode? broadcastMode,
            NetworkPacket networkPacket)
        {
            switch (broadcastMode)
            {
                case BroadcastMode.Room:
                    await _roomManager
                        .ApplyAsync(
                            roomId: roomId ?? throw new ArgumentNullException(nameof(roomId)),
                            condition: (connection) => true,
                            func: (connection) => Send(connection, udpSender, ref networkPacket))
                        .ConfigureAwait(false);

                    break;
                case BroadcastMode.RoomExceptCaller:
                    await _roomManager
                        .ApplyAsync(
                            roomId: roomId ?? throw new ArgumentNullException(nameof(roomId)),
                            condition: (connection) => connection.ConnectionId != networkPacket.ConnectionId,
                            func: (connection) => Send(connection, udpSender, ref networkPacket))
                        .ConfigureAwait(false);

                    break;
                case BroadcastMode.Caller:
                    await Send(
                            connection: _connectionPool.TryGetConnection(networkPacket.ConnectionId),
                            udpSender: udpSender,
                            networkPacket: ref networkPacket)
                        .ConfigureAwait(false);

                    break;
                case BroadcastMode.Server:
                    await udpSender
                        .SendAsync(ref networkPacket)
                        .ConfigureAwait(false);

                    break;
                case BroadcastMode.AllPeers:
                    await _connectionPool
                        .Apply(
                            condition: () => true,
                            func: () => udpSender.SendAsync(ref networkPacket))
                        .ConfigureAwait(false);

                    break;

                default:
                    throw new NotSupportedException(broadcastMode.ToString());
            }
        }

        private Task Send(
            IConnection connection,
            IUdpSender udpSender,
            ref NetworkPacket networkPacket)
        {
            // produce packet
            var packet = new NetworkPacket(
                hookId: networkPacket.HookId,
                channelType: networkPacket.ChannelType,
                networkPacketType: NetworkPacketType.FromServer, // packetType
                connectionId: connection.ConnectionId,
                serializer: networkPacket.Serializer,
                createdAt: DateTimeOffset.UtcNow, // createdAt ?
                ipEndPoint: connection.GetIp());

            return udpSender.SendAsync(ref packet);
        }
    }
}
