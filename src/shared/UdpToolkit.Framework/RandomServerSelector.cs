namespace UdpToolkit.Framework
{
    using System;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;

    public class RandomServerSelector : IServerSelector, IRawServerSelector
    {
        private static readonly Random Random = new Random();
        private readonly Peer[] _servers;

        public RandomServerSelector(
            IPEndPoint[] inputIps)
        {
            var serverId = Guid.NewGuid();

            _servers = inputIps
                .Select(ip => Peer.New(
                    inactivityTimeout: TimeSpan.MaxValue,
                    peerId: serverId,
                    peerIps: inputIps.ToList()))
                .ToArray();
        }

        public IPeer GetServer()
        {
            return _servers[Random.Next(0, _servers.Length - 1)];
        }

        Peer IRawServerSelector.GetServer()
        {
            return _servers[Random.Next(0, _servers.Length - 1)];
        }
    }
}