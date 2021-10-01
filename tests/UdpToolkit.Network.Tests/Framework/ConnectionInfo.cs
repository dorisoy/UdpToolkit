namespace UdpToolkit.Network.Tests.Framework
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    internal class ConnectionInfo
    {
        internal ConnectionInfo(
            IpV4Address ip,
            Guid connectionId)
        {
            Ip = ip;
            ConnectionId = connectionId;
        }

        public IpV4Address Ip { get; }

        public Guid ConnectionId { get; }
    }
}