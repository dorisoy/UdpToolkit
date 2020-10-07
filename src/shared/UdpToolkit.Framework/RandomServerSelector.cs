namespace UdpToolkit.Framework
{
    using System;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;

    public class RandomServerSelector : IServerSelector
    {
        private static readonly Random Random = new Random();
        private readonly Peer[] _servers;

        public RandomServerSelector(
            IPEndPoint[] servers)
        {
            _servers = servers
                .Select(ip => Peer.New(peerId: Guid.NewGuid(), peerIps: servers.ToList()))
                .ToArray();
        }

        public IPeer GetServer()
        {
            return _servers[Random.Next(0, _servers.Length - 1)];
        }
    }
}