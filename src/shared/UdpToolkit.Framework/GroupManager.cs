namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Events;
    using UdpToolkit.Network.Contracts.Connections;

    /// <inheritdoc />
    public sealed class GroupManager : IGroupManager
    {
        private readonly IConnectionPool _connectionPool;
        private readonly ConcurrentDictionary<Guid, Group> _groups = new ConcurrentDictionary<Guid, Group>();
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _groupTtl;
        private readonly Timer _houseKeeper;
        private readonly IHostEventReporter _hostEventReporter;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupManager"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        /// <param name="groupTtl">Group ttl.</param>
        /// <param name="scanFrequency">Scan frequency for cleanup inactive groups.</param>
        /// <param name="hostEventReporter">Instance of host event reporter.</param>
        /// <param name="connectionPool">Instance of connection pool.</param>
        public GroupManager(
            IDateTimeProvider dateTimeProvider,
            TimeSpan groupTtl,
            TimeSpan scanFrequency,
            IHostEventReporter hostEventReporter,
            IConnectionPool connectionPool)
        {
            _dateTimeProvider = dateTimeProvider;
            _groupTtl = groupTtl;
            _hostEventReporter = hostEventReporter;
            _connectionPool = connectionPool;
            _houseKeeper = new Timer(
                callback: ScanForCleaningInactiveGroups,
                state: null,
                dueTime: TimeSpan.FromSeconds(10),
                period: scanFrequency);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GroupManager"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~GroupManager()
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
        public void JoinOrCreate(
            Guid groupId,
            Guid connectionId)
        {
            if (_connectionPool.TryGetConnection(connectionId, out var connection))
            {
                _groups.AddOrUpdate(
                    key: groupId,
                    addValueFactory: (id) => new Group(
                        id: id,
                        groupConnections: new List<IConnection>
                        {
                            connection,
                        },
                        createdAt: _dateTimeProvider.GetUtcNow()),
                    updateValueFactory: (id, group) =>
                    {
                        if (group.GroupConnections.All(x => x.ConnectionId != connectionId))
                        {
                            group.GroupConnections.Add(connection);
                        }

                        return group;
                    });
            }
        }

        /// <inheritdoc />
        public void Leave(Guid groupId, Guid connectionId)
        {
            if (_groups.TryGetValue(groupId, out var group) && _connectionPool.TryGetConnection(connectionId, out var connection))
            {
                group.GroupConnections.Remove(connection);
            }
        }

        /// <inheritdoc />
        public Group GetGroup(Guid groupId)
        {
            _groups.TryGetValue(groupId, out var group);
            return group;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _houseKeeper?.Dispose();
            }

            _disposed = true;
        }

        private void ScanForCleaningInactiveGroups(object state)
        {
            var groupsCount = _groups.Count;
            var scanExpiredGroupsStarted = new ScanExpiredGroupsStarted(groupsCount);

            _hostEventReporter.Handle(in scanExpiredGroupsStarted);
            var now = _dateTimeProvider.GetUtcNow();
            for (var i = 0; i < groupsCount; i++)
            {
                var group = _groups.ElementAt(i);

                var ttlDiff = now - group.Value.CreatedAt;
                if (ttlDiff > _groupTtl)
                {
                    _groups.TryRemove(group.Key, out _);
                }
            }
        }
    }
}