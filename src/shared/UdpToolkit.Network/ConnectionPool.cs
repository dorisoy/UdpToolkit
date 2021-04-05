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

        public IConnection GetOrAdd(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            IPEndPoint ip)
        {
            var newConnection = Connection.New(
                keepAlive: keepAlive,
                lastHeartbeat: lastHeartbeat,
                connectionId: connectionId,
                ipEndPoint: ip);

            return _connections.GetOrAdd(connectionId, newConnection);
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
            _logger.Debug($"Cleanup inactive connections");
            var now = _dateTimeProvider.GetUtcNow();
            for (var i = 0; i < _connections.Count; i++)
            {
                var connection = _connections.ElementAt(i);
                if (connection.Value.KeepAlive)
                {
                    continue;
                }

                var inactivityDiff = now - connection.Value.LastHeartbeat;
                if (inactivityDiff > _inactivityTimeout)
                {
                    _connections.TryRemove(connection.Key, out _);
                }
            }
        }
    }
}