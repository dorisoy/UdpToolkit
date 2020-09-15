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
        private readonly Guid _me;

        public DatagramBuilder(
            Guid me,
            IPeerManager peerManager,
            IRoomManager roomManager,
            IServerSelector serverSelector)
        {
            _me = me;
            _peerManager = peerManager;
            _roomManager = roomManager;
            _serverSelector = serverSelector;
        }

        public Datagram<TEvent> ToServer<TEvent>(TEvent @event, byte hookId)
        {
            var serverIp = _serverSelector.GetServer();

            return new Datagram<TEvent>(@event, new[] { new ShortPeer(peerId: _me, ipEndPoint: serverIp),  }, hookId);
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
            var peer = _peerManager.Get(peerId);

            return new Datagram<TEvent>(@event, new[] { new ShortPeer(peer.PeerId, peer.GetRandomIp()) }, hookId);
        }
    }
}