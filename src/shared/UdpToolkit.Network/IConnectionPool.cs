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

        IConnection AddOrUpdate(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            List<IPEndPoint> ips);

        void Apply(
            Action<IConnection> action);
    }
}