namespace UdpToolkit.Network
{
    using System;
    using UdpToolkit.Network.Sockets;

    public interface IConnectionPool : IDisposable
    {
        void Remove(
            IConnection connection);

        bool TryGetConnection(
            Guid connectionId,
            out IConnection connection);

        IConnection GetOrAdd(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            IpV4Address ipAddress);

        void Apply(
            Action<IConnection> action);
    }
}