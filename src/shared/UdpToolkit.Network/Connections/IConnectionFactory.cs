namespace UdpToolkit.Network.Connections
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    internal interface IConnectionFactory
    {
        IConnection Create(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            IpV4Address ipAddress);
    }
}