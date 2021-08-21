namespace UdpToolkit.Network.Clients
{
    using System;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Utils;

    public class UdpClientFactory : IUdpClientFactory
    {
        private readonly IConnectionIdFactory _connectionIdFactory;
        private readonly UdpClientSettings _udpClientSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUdpToolkitLoggerFactory _loggerFactory;
        private readonly Lazy<IConnectionPool> _lazyConnectionPool;
        private readonly Lazy<IResendQueue> _lazyResendQueue;

        public UdpClientFactory(
            UdpClientSettings udpClientSettings,
            ConnectionPoolSettings connectionPoolSettings,
            IUdpToolkitLoggerFactory loggerFactory,
            IConnectionIdFactory connectionIdFactory,
            IDateTimeProvider dateTimeProvider = null)
        {
            _udpClientSettings = udpClientSettings;
            _loggerFactory = loggerFactory;
            _connectionIdFactory = connectionIdFactory;
            _lazyResendQueue = new Lazy<IResendQueue>(() => new ResendQueue());
            _lazyConnectionPool = new Lazy<IConnectionPool>(() =>
            {
                var connectionFactory = new ConnectionFactory(
                    channelsFactory: _udpClientSettings.ChannelsFactory);

                return new ConnectionPool(
                    dateTimeProvider: _dateTimeProvider,
                    logger: _loggerFactory.Create<ConnectionPool>(),
                    settings: connectionPoolSettings,
                    connectionFactory: connectionFactory);
            });
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
        }

        public IUdpClient Create(
            IpV4Address ipV4Address)
        {
            return new UdpClient(
                connectionIdFactory: _connectionIdFactory,
                connectionPool: _lazyConnectionPool.Value,
                logger: _loggerFactory.Create<UdpClient>(),
                dateTimeProvider: _dateTimeProvider,
                client: _udpClientSettings.SocketFactory.Create(ipV4Address),
                resendQueue: _lazyResendQueue.Value,
                settings: _udpClientSettings);
        }
    }
}