namespace UdpToolkit.Network.Contracts.Connections
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public interface IConnectionFactory
    {
        IConnection Create(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            IpV4Address ipAddress);
    }
}