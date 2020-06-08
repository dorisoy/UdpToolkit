namespace UdpToolkit.Framework.Server.Peers
{
    using System;
    using System.Linq;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public class HubClients : IHubClients
    {
        private readonly IPeerManager _peerManager;
        private readonly IRoomManager _roomManager;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly ISerializer _serializer;

        public HubClients(
            IPeerManager peerManager,
            IRoomManager roomManager,
            IAsyncQueue<NetworkPacket> outputQueue,
            ISerializer serializer)
        {
            _peerManager = peerManager;
            _roomManager = roomManager;
            _outputQueue = outputQueue;
            _serializer = serializer;
        }

        public IPeerProxy All()
        {
            var peers = _peerManager.GetAll();

            return new PeerProxy(peers: peers, outputQueue: _outputQueue, serializer: _serializer);
        }

        public IPeerProxy AllExcept(Guid peerId)
        {
            var peers = _peerManager
                .GetAll()
                .Where(x => x.PeerId != peerId);

            return new PeerProxy(peers: peers, outputQueue: _outputQueue, serializer: _serializer);
        }

        public IPeerProxy Room(byte roomId)
        {
            var peers = _roomManager
                .GetRoom(roomId)
                .GetPeers();

            return new PeerProxy(peers: peers, outputQueue: _outputQueue, serializer: _serializer);
        }

        public IPeerProxy RoomExcept(byte roomId, Guid peerId)
        {
            var peers = _roomManager
                .GetRoom(roomId)
                .GetPeers()
                .Where(x => x.PeerId != peerId);

            return new PeerProxy(peers: peers, outputQueue: _outputQueue, serializer: _serializer);
        }

        public IPeerProxy Caller(byte roomId, Guid peerId)
        {
            var peer = _roomManager
                .GetRoom(roomId)
                .GetPeer(peerId);

            return new PeerProxy(peers: new[] { peer }, outputQueue: _outputQueue, serializer: _serializer);
        }
    }
}