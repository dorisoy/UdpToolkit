namespace UdpToolkit.Framework
{
    using System;
    using System.Net;
    using UdpToolkit.Core;

    public class RandomServerSelector : IServerSelector
    {
        private static readonly Random Random = new Random();
        private readonly IPEndPoint[] _servers;

        public RandomServerSelector(
            IPEndPoint[] servers)
        {
            _servers = servers;
        }

        public IPEndPoint GetServer()
        {
            return _servers[Random.Next(0, _servers.Length - 1)];
        }
    }
}