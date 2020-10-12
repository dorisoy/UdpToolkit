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
        private readonly IPEndPoint[] _outputIps;

        public RandomServerSelector(
            IPEndPoint[] inputIps,
            IPEndPoint[] outputIps)
        {
            var serverId = Guid.NewGuid();
            _outputIps = outputIps;

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

        public bool IsServerIp(IPEndPoint ipEndPoint) => _outputIps.Contains(ipEndPoint);
    }
}