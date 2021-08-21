namespace UdpToolkit.Network.Connections
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Utils;

    internal sealed class ConnectionPool : IConnectionPool
    {
        private readonly ConcurrentDictionary<Guid, IConnection> _connections = new ConcurrentDictionary<Guid, IConnection>();
        private readonly IUdpToolkitLogger _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConnectionFactory _connectionFactory;
        private readonly TimeSpan _inactivityTimeout;
        private readonly Timer _housekeeper;

        private bool _disposed = false;

        internal ConnectionPool(
            IDateTimeProvider dateTimeProvider,
            IUdpToolkitLogger logger,
            ConnectionPoolSettings settings,
            IConnectionFactory connectionFactory)
        {
            _dateTimeProvider = dateTimeProvider;
            _inactivityTimeout = settings.ConnectionTimeout;
            _connectionFactory = connectionFactory;
            _logger = logger;
            _housekeeper = new Timer(
                callback: ScanForCleaningInactiveConnections,
                state: null,
                dueTime: TimeSpan.FromSeconds(10),
                period: settings.ConnectionsCleanupFrequency);
        }

        ~ConnectionPool()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            IpV4Address ipV4Address)
        {
            var newConnection = _connectionFactory.Create(
                keepAlive: keepAlive,
                lastHeartbeat: lastHeartbeat,
                connectionId: connectionId,
                ipAddress: ipV4Address);

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

        private void ScanForCleaningInactiveConnections(object state)
        {
#if DEBUG
            _logger.Debug($"[UdpToolkit.Network] Cleanup inactive connections");
#endif

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

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _housekeeper?.Dispose();
            }

            _disposed = true;
        }
    }
}