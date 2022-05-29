namespace UdpToolkit.Network.Clients
{
    using System.Buffers;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Utils;

    /// <inheritdoc />
    public class UdpClientFactory : IUdpClientFactory
    {
        private readonly UdpClientSettings _udpClientSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConnectionPool _lazyConnectionPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClientFactory"/> class.
        /// </summary>
        /// <param name="udpClientSettings">Instance of UDP client settings.</param>
        /// <param name="connectionPool">Instance of connection pool.</param>
        /// <param name="loggerFactory">Instance of logger factory.</param>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        public UdpClientFactory(
            UdpClientSettings udpClientSettings,
            IConnectionPool connectionPool,
            ILoggerFactory loggerFactory,
            IDateTimeProvider dateTimeProvider = null)
        {
            _udpClientSettings = udpClientSettings;
            _loggerFactory = loggerFactory;
            _lazyConnectionPool = connectionPool;
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
        }

        /// <inheritdoc />
        public unsafe IUdpClient Create(
            IpV4Address ipV4Address)
        {
            var packetsPool = new ConcurrentPool<InNetworkPacket>(
                factory: (pool) => new InNetworkPacket(
                    arrayPool: ArrayPool<byte>.Shared,
                    networkPacketsPool: pool),
                initSize: _udpClientSettings.PacketsPoolSize);

            return new UdpClient(
                connectionPool: _lazyConnectionPool,
                logger: _loggerFactory.Create<UdpClient>(),
                dateTimeProvider: _dateTimeProvider,
                client: _udpClientSettings.SocketFactory.Create(ipV4Address),
                settings: _udpClientSettings,
                arrayPool: ArrayPool<byte>.Shared,
                packetsPool: packetsPool);
        }
    }
}