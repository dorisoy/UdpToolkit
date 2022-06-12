namespace UdpToolkit.Network.Connections
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Events;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Utils;

    /// <inheritdoc />
    public sealed class ConnectionPool : IConnectionPool
    {
        private readonly ConcurrentDictionary<Guid, IConnection> _connections = new ConcurrentDictionary<Guid, IConnection>();
        private readonly INetworkEventReporter _networkEventReporter;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConnectionFactory _connectionFactory;
        private readonly TimeSpan _inactivityTimeout;
        private readonly Timer _housekeeper;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPool"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        /// <param name="networkEventReporter">Instance of network event reporter.</param>
        /// <param name="settings">Instance of settings.</param>
        /// <param name="connectionFactory">Instance of connection factory.</param>
        public ConnectionPool(
            IDateTimeProvider dateTimeProvider,
            INetworkEventReporter networkEventReporter,
            ConnectionPoolSettings settings,
            IConnectionFactory connectionFactory)
        {
            _dateTimeProvider = dateTimeProvider;
            _inactivityTimeout = settings.ConnectionTimeout;
            _connectionFactory = connectionFactory;
            _networkEventReporter = networkEventReporter;
            _housekeeper = new Timer(
                callback: ScanForCleaningInactiveConnections,
                state: null,
                dueTime: TimeSpan.FromSeconds(10),
                period: settings.ConnectionsCleanupFrequency);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConnectionPool"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~ConnectionPool()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Remove(
            IConnection connection)
        {
            _connections.TryRemove(connection.ConnectionId, out _);
        }

        /// <inheritdoc />
        public bool TryGetConnection(
            Guid connectionId,
            out IConnection connection)
        {
            return _connections.TryGetValue(connectionId, out connection);
        }

        /// <inheritdoc />
        public IConnection GetOrAdd(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset timestamp,
            IpV4Address ipV4Address)
        {
            // MEMORY OPTIMIZATION: avoid usage GetOrAdd method
            // https://www.meziantou.net/concurrentdictionary-closure.html
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                return connection;
            }

            var newConnection = _connectionFactory.Create(connectionId, keepAlive, timestamp, ipV4Address);
            if (_connections.TryAdd(connectionId, newConnection))
            {
                _networkEventReporter.Handle(new ConnectionAccepted(ipV4Address));

                return newConnection;
            }

            return null;
        }

        /// <inheritdoc />
        public IReadOnlyList<IConnection> GetAll()
        {
            return _connections.Values.ToList();
        }

        private void ScanForCleaningInactiveConnections(object state)
        {
            var now = _dateTimeProvider.GetUtcNow();
            _networkEventReporter.Handle(new ScanInactiveConnectionsStarted(now));

            for (var i = 0; i < _connections.Count; i++)
            {
                var connection = _connections.ElementAt(i);
                if (connection.Value.KeepAlive)
                {
                    continue;
                }

                var inactivityDiff = now - connection.Value.LastActivityAt;
                if (inactivityDiff > _inactivityTimeout && _connections.TryRemove(connection.Key, out _))
                {
                    _networkEventReporter.Handle(new ConnectionRemovedByTimeout(connection.Key, inactivityDiff));
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
                _housekeeper.Dispose();
            }

            _disposed = true;
        }
    }
}