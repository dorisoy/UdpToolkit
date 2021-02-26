namespace UdpToolkit.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    public interface IConnectionPool
    {
        void Remove(
            IConnection connection);

        IConnection TryGetConnection(
            Guid connectionId);

        IConnection AddOrUpdate(
            Guid connectionId,
            List<IPEndPoint> ips,
            TimeSpan connectionTimeout);

        Task Apply(
            Func<bool> condition,
            Func<Task> func);
    }
}