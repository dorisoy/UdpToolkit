namespace UdpToolkit.Network.Clients
{
    using System;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Utils;

    public class UdpClientFactory : IUdpClientFactory
    {
        private readonly NetworkSettings _networkSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUdpToolkitLoggerFactory _loggerFactory;
        private readonly Lazy<IConnectionPool> _lazyConnectionPool;
        private readonly Lazy<IResendQueue> _lazyResendQueue;

        public UdpClientFactory(
            NetworkSettings networkSettings,
            IUdpToolkitLoggerFactory loggerFactory,
            IDateTimeProvider dateTimeProvider = null)
        {
            _networkSettings = networkSettings;
            _loggerFactory = loggerFactory;
            _lazyResendQueue = new Lazy<IResendQueue>(() => new ResendQueue());
            _lazyConnectionPool = new Lazy<IConnectionPool>(() =>
            {
                var connectionFactory = new ConnectionFactory(
                    channelsFactory: _networkSettings.ChannelsFactory);

                return new ConnectionPool(
                    dateTimeProvider: _dateTimeProvider,
                    logger: _loggerFactory.Create<ConnectionPool>(),
                    networkSettings: _networkSettings,
                    connectionFactory: connectionFactory);
            });
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
        }

        public IUdpClient Create(
            IpV4Address ipV4Address)
        {
            return new UdpClient(
                connectionPool: _lazyConnectionPool.Value,
                logger: _loggerFactory.Create<UdpClient>(),
                dateTimeProvider: _dateTimeProvider,
                client: _networkSettings.SocketFactory.Create(ipV4Address),
                resendQueue: _lazyResendQueue.Value,
                networkSettings: _networkSettings);
        }

        public TimeSpan? GetRtt(
            Guid connectionId)
        {
            _lazyConnectionPool.Value.TryGetConnection(connectionId, out var connection);
            return connection?.GetRtt();
        }

        public void BootstrapConnection(
            Guid connectionId,
            IpV4Address ipV4Address)
        {
            _lazyConnectionPool.Value.GetOrAdd(
                connectionId: connectionId,
                keepAlive: true,
                lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                ipV4Address: ipV4Address);
        }
    }
}