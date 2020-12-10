namespace UdpToolkit.Framework
{
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;

    public sealed class BroadcastManager : IBroadcastManager
    {
        private readonly IRawPeerManager _rawPeerManager;
        private readonly IRawRoomManager _rawRoomManager;
        private readonly IRawServerSelector _serverSelector;

        public BroadcastManager(
            IRawPeerManager rawPeerManager,
            IRawRoomManager rawRoomManager,
            IRawServerSelector serverSelector)
        {
            _rawPeerManager = rawPeerManager;
            _rawRoomManager = rawRoomManager;
            _serverSelector = serverSelector;
        }

        public Task AckToServer(
            IUdpSender udpSender,
            NetworkPacket networkPacket)
        {
            var serverPeer = _serverSelector
                .GetServer();

            var ackPacket = serverPeer
                .GetChannel(networkPacket.ChannelType)
                .GetAck(networkPacket);

            ackPacket.SetIp(serverPeer.GetRandomIp());

            return udpSender.SendAsync(ackPacket);
        }

        public Task Caller(
            IUdpSender udpSender,
            NetworkPacket networkPacket)
        {
            var peer = _rawPeerManager
                .GetPeer(networkPacket.PeerId);

            return Send(udpSender, peer, networkPacket);
        }

        public Task Room(
            int roomId,
            IUdpSender udpSender,
            NetworkPacket networkPacket)
        {
            return _rawRoomManager
                .Apply(
                    caller: networkPacket.PeerId,
                    roomId: roomId,
                    condition: (peer) => true,
                    func: (peer) => Send(udpSender, peer, networkPacket));
        }

        public Task RoomExceptCaller(
            int roomId,
            IUdpSender udpSender,
            NetworkPacket networkPacket)
        {
            return _rawRoomManager
                .Apply(
                    caller: networkPacket.PeerId,
                    roomId: roomId,
                    condition: (peer) => peer.PeerId != networkPacket.PeerId,
                    func: (peer) => Send(udpSender, peer, networkPacket));
        }

        public Task AllServer(
            IUdpSender udpSender,
            NetworkPacket networkPacket)
        {
            return _rawPeerManager
                .Apply(
                    condition: (peer) => true,
                    action: (peer) => Send(udpSender, peer, networkPacket));
        }

        public async Task Server(
            IUdpSender udpSender,
            NetworkPacket networkPacket)
        {
            var serverPeer = _serverSelector.GetServer();
            networkPacket.SetIp(serverPeer.GetRandomIp());

            _rawPeerManager
                .GetPeer(networkPacket.PeerId)
                .GetChannel(networkPacket.ChannelType)
                .HandleOutputPacket(networkPacket);

            await udpSender
                .SendAsync(networkPacket)
                .ConfigureAwait(false);
        }

        private Task Send(
            IUdpSender udpSender,
            Peer peer,
            NetworkPacket networkPacket)
        {
            if (peer.PeerId == networkPacket.PeerId && networkPacket.IsReliable)
            {
                // produce ack
                var ackPacket = peer
                    .GetChannel(networkPacket.ChannelType)
                    .GetAck(networkPacket);

                ackPacket.SetIp(peer.GetRandomIp());

                return udpSender.SendAsync(ackPacket);
            }
            else
            {
                // produce packet
                var newPacket = networkPacket
                    .Clone(
                        peerId: peer.PeerId,
                        ipEndPoint: peer.GetRandomIp(), // networkPacket.NetworkPacketType
                        networkPacketType: NetworkPacketType.FromServer);

                peer
                    .GetChannel(channelType: newPacket.ChannelType)
                    .GetNext(networkPacket: newPacket);

                return udpSender.SendAsync(newPacket);
            }
        }
    }
}