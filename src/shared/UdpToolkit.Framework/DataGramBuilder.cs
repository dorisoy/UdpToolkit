namespace UdpToolkit.Framework
{
    using System;
    using System.Linq;
    using UdpToolkit.Core;

    public class DataGramBuilder : IDataGramBuilder
    {
        private readonly IPeerManager _peerManager;
        private readonly IRoomManager _roomManager;

        public DataGramBuilder(
            IPeerManager peerManager,
            IRoomManager roomManager)
        {
            _peerManager = peerManager;
            _roomManager = roomManager;
        }

        public DataGram<TResp> All<TResp>(TResp response, byte hookId)
        {
            var peers = _peerManager
                .GetAll()
                .Select(x => new ShortPeer(x.PeerId, x.GetRandomIp()));

            return new DataGram<TResp>(response, peers, hookId);
        }

        public DataGram<TResp> AllExcept<TResp>(TResp response, Guid peerId, byte hookId)
        {
            var peers = _peerManager
                .GetAll()
                .Where(x => x.PeerId != peerId)
                .Select(x => new ShortPeer(x.PeerId, x.GetRandomIp()));

            return new DataGram<TResp>(response, peers, hookId);
        }

        public DataGram<TResp> Room<TResp>(TResp response, byte roomId, byte hookId)
        {
            var peers = _roomManager
                .GetRoom(roomId)
                .GetPeers()
                .Select(x => new ShortPeer(x.PeerId, x.GetRandomIp()));

            return new DataGram<TResp>(response, peers, hookId);
        }

        public DataGram<TResp> RoomExcept<TResp>(TResp response, byte roomId, Guid peerId, byte hookId)
        {
            var peers = _roomManager
                .GetRoom(roomId)
                .GetPeers()
                .Where(x => x.PeerId != peerId)
                .Select(x => new ShortPeer(x.PeerId, x.GetRandomIp()));

            return new DataGram<TResp>(response, peers, hookId);
        }

        public DataGram<TResp> Caller<TResp>(TResp response, byte roomId, Guid peerId, byte hookId)
        {
            var peer = _roomManager
                .GetRoom(roomId)
                .GetPeer(peerId);

            return new DataGram<TResp>(response, new[] { new ShortPeer(peer.PeerId, peer.GetRandomIp()) }, hookId);
        }

        public DataGram<TResp> Caller<TResp>(TResp response, Guid peerId, byte hookId)
        {
            var peer = _peerManager.Get(peerId);

            return new DataGram<TResp>(response, new[] { new ShortPeer(peer.PeerId, peer.GetRandomIp()) }, hookId);
        }
    }
}