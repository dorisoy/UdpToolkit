namespace UdpToolkit.Framework
{
    using System;
    using System.Linq;
    using UdpToolkit.Core;

    public class DatagramBuilder : IDatagramBuilder
    {
        private readonly IPeerManager _peerManager;
        private readonly IRoomManager _roomManager;
        private readonly IServerSelector _serverSelector;
        private readonly ServerHostClient _serverHostClient;

        public DatagramBuilder(
            IPeerManager peerManager,
            IRoomManager roomManager,
            IServerSelector serverSelector,
            ServerHostClient serverHostClient)
        {
            _peerManager = peerManager;
            _roomManager = roomManager;
            _serverSelector = serverSelector;
            _serverHostClient = serverHostClient;
        }

        public Datagram<TEvent> ToServer<TEvent>(TEvent @event, byte hookId)
        {
            var serverIp = _serverSelector.GetServer();

            return new Datagram<TEvent>(@event, new[] { new ShortPeer(peerId: _serverHostClient.PeerId, ipEndPoint: serverIp),  }, hookId);
        }

        public Datagram<TEvent> All<TEvent>(TEvent @event, byte hookId)
        {
            var peers = _peerManager
                .GetAll()
                .Select(x => new ShortPeer(x.PeerId, x.GetRandomIp()));

            return new Datagram<TEvent>(@event, peers, hookId);
        }

        public Datagram<TEvent> AllExcept<TEvent>(TEvent @event, Guid peerId, byte hookId)
        {
            var peers = _peerManager
                .GetAll()
                .Where(x => x.PeerId != peerId)
                .Select(x => new ShortPeer(x.PeerId, x.GetRandomIp()));

            return new Datagram<TEvent>(@event, peers, hookId);
        }

        public Datagram<TEvent> Room<TEvent>(TEvent @event, byte roomId, byte hookId)
        {
            var peers = _roomManager
                .GetRoom(roomId)
                .GetPeers()
                .Select(x => new ShortPeer(x.PeerId, x.GetRandomIp()));

            return new Datagram<TEvent>(@event, peers, hookId);
        }

        public Datagram<TEvent> RoomExcept<TEvent>(TEvent @event, byte roomId, Guid peerId, byte hookId)
        {
            var peers = _roomManager
                .GetRoom(roomId)
                .GetPeers()
                .Where(x => x.PeerId != peerId)
                .Select(x => new ShortPeer(x.PeerId, x.GetRandomIp()));

            return new Datagram<TEvent>(@event, peers, hookId);
        }

        public Datagram<TEvent> Caller<TEvent>(TEvent @event, byte roomId, Guid peerId, byte hookId)
        {
            var peer = _roomManager
                .GetRoom(roomId)
                .GetPeer(peerId);

            return new Datagram<TEvent>(@event, new[] { new ShortPeer(peer.PeerId, peer.GetRandomIp()) }, hookId);
        }

        public Datagram<TEvent> Caller<TEvent>(TEvent @event, Guid peerId, byte hookId)
        {
            var exists = _peerManager.TryGetPeer(peerId, out var peer); // TODO

            return new Datagram<TEvent>(@event, new[] { new ShortPeer(peer.PeerId, peer.GetRandomIp()) }, hookId);
        }
    }
}