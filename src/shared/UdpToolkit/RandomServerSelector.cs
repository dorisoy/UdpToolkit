namespace UdpToolkit
{
    using System;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Peers;

    public sealed class RandomServerSelector : IServerSelector, IRawServerSelector
    {
        private readonly IPeerManager _peerManager;
        private readonly Guid _serverId;

        public RandomServerSelector(
            IPEndPoint[] inputIps,
            PeerManager peerManager)
        {
            _peerManager = peerManager;
            _serverId = Guid.NewGuid();

            peerManager.AddOrUpdate(_serverId, inputIps.ToList(), TimeSpan.MaxValue);
        }

        IPeer IServerSelector.GetServer()
        {
            return GetServer();
        }

        IRawPeer IRawServerSelector.GetServer()
        {
            return GetServer();
        }

        private Peer GetServer()
        {
            if (_peerManager.TryGetPeer(_serverId, out var peer))
            {
                return peer as Peer;
            }

            return null;
        }
    }
}