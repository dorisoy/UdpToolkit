namespace UdpToolkit.Network
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Utils;

    public sealed class ConnectionPool : IConnectionPool
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ConcurrentDictionary<Guid, IConnection> _connections = new ConcurrentDictionary<Guid, IConnection>();

        public ConnectionPool(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public void Remove(
            IConnection connection)
        {
            _connections.TryRemove(connection.ConnectionId, out _);
        }

        public IConnection TryGetConnection(
            Guid connectionId)
        {
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                return connection;
            }

            return null;
        }

        public IConnection AddOrUpdate(
            Guid connectionId,
            List<IPEndPoint> ips,
            TimeSpan connectionTimeout)
        {
            return _connections.AddOrUpdate(
                key: connectionId,
                addValueFactory: (key) => Connection.New(
                    connectionId: connectionId,
                    ipEndPoints: ips,
                    connectionTimeout: connectionTimeout),
                updateValueFactory: (key, connection) =>
                {
                    connection
                        .OnActivity(lastActivityAt: _dateTimeProvider.GetUtcNow());

                    foreach (var ip in ips)
                    {
                        if (!connection.IpEndPoints.Contains(ip))
                        {
                            connection.IpEndPoints.Add(ip);
                        }
                    }

                    return connection;
                });
        }

        public async Task Apply(
            Func<bool> condition,
            Func<Task> func)
        {
            for (var i = 0; i < _connections.Count; i++)
            {
                var pair = _connections.ElementAt(i);
                if (!condition())
                {
                    continue;
                }

                await func().ConfigureAwait(false);
            }
        }
    }
}