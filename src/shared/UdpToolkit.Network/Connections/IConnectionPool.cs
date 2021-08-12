namespace UdpToolkit.Network.Connections
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    internal interface IConnectionPool : IDisposable
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
            IpV4Address ipV4Address);

        void Apply(
            Action<IConnection> action);
    }
}