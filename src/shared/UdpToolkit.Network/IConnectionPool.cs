namespace UdpToolkit.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;

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
            IPEndPoint ip);

        void Apply(
            Action<IConnection> action);
    }
}