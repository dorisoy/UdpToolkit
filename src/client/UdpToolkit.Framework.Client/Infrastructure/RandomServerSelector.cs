namespace UdpToolkit.Framework.Client.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Framework.Client.Core;
    using UdpToolkit.Network.Peers;

    public sealed class RandomServerSelector : IServerSelector
    {
        private static readonly Random Random = new Random();
        private readonly Peer[] _servers;

        public RandomServerSelector(IEnumerable<Peer> servers)
        {
            _servers = servers.ToArray();
        }

        public Peer GetServer()
        {
            return _servers[Random.Next(0, _servers.Length - 1)];
        }
    }
}