namespace UdpToolkit.Network
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Utils;

    public sealed class ConnectionPool : IConnectionPool
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _inactivityTimeout;
        private readonly Timer _housekeeper;
        private readonly ConcurrentDictionary<Guid, IConnection> _connections = new ConcurrentDictionary<Guid, IConnection>();

        public ConnectionPool(
            IDateTimeProvider dateTimeProvider,
            IUdpToolkitLogger logger,
            TimeSpan scanFrequency,
            TimeSpan inactivityTimeout)
        {
            _dateTimeProvider = dateTimeProvider;
            _inactivityTimeout = inactivityTimeout;
            _housekeeper = new Timer(
                callback: ScanForCleaningInactiveConnections,
                state: null,
                dueTime: TimeSpan.FromSeconds(10),
                period: scanFrequency);
            _logger = logger;
        }

        public void Remove(
            IConnection connection)
        {
            _connections.TryRemove(connection.ConnectionId, out _);
        }

        public bool TryGetConnection(
            Guid connectionId,
            out IConnection connection)
        {
            return _connections.TryGetValue(connectionId, out connection);
        }

        public IConnection AddOrUpdate(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            List<IPEndPoint> ips)
        {
            return _connections.AddOrUpdate(
                key: connectionId,
                addValueFactory: (key) => Connection.New(
                    keepAlive: keepAlive,
                    lastHeartbeat: lastHeartbeat,
                    connectionId: connectionId,
                    ipEndPoints: ips),
                updateValueFactory: (key, connection) =>
                {
                    for (var i = 0; i < ips.Count; i++)
                    {
                        var ip = ips[i];
                        if (!connection.IpEndPoints.Contains(ip))
                        {
                            connection.IpEndPoints.Add(ip);
                        }
                    }

                    return connection;
                });
        }

        public void Apply(
            Action<IConnection> action)
        {
            for (var i = 0; i < _connections.Count; i++)
            {
                var pair = _connections.ElementAt(i);
                if (pair.Value == null)
                {
                    continue;
                }

                action(pair.Value);
            }
        }

        public void Dispose()
        {
            _housekeeper?.Dispose();
        }

        private void ScanForCleaningInactiveConnections(object state)
        {
            var now = _dateTimeProvider.GetUtcNow();
            for (var i = 0; i < _connections.Count; i++)
            {
                var connection = _connections.ElementAt(i);
                if (connection.Value.KeepAlive)
                {
                    _logger.Debug($"keep alive - {connection.Key}");
                    continue;
                }

                var inactivityDiff = now - connection.Value.LastHeartbeat;
                _logger.Debug($"Inactivity diff - {inactivityDiff} timeout {_inactivityTimeout} last heartbeat - {connection.Value.LastHeartbeat}");
                if (inactivityDiff > _inactivityTimeout)
                {
                    _logger.Debug($"Remove connection {connection.Key}");
                    _connections.TryRemove(connection.Key, out _);
                }
            }
        }
    }
}