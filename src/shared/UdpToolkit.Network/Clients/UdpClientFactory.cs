namespace UdpToolkit.Network.Clients
{
    using System.Buffers;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Utils;

    /// <inheritdoc />
    public class UdpClientFactory : IUdpClientFactory
    {
        private readonly INetworkSettings _networkSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConnectionPool _connectionPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClientFactory"/> class.
        /// </summary>
        /// <param name="networkSettings">Instance of UDP client settings.</param>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        public UdpClientFactory(
            INetworkSettings networkSettings,
            IDateTimeProvider dateTimeProvider = null)
        {
            _connectionPool = new ConnectionPool(
                dateTimeProvider: new Network.Utils.DateTimeProvider(),
                networkEventReporter: networkSettings.NetworkEventReporter,
                settings: new ConnectionPoolSettings(
                    connectionTimeout: networkSettings.ConnectionTimeout,
                    connectionsCleanupFrequency: networkSettings.ConnectionsCleanupFrequency),
                connectionFactory: new ConnectionFactory(networkSettings.ChannelsFactory));

            _networkSettings = networkSettings;
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
                initSize: _networkSettings.PacketsPoolSize);

            return new UdpClient(
                connectionPool: _connectionPool,
                networkEventReporter: this._networkSettings.NetworkEventReporter,
                dateTimeProvider: _dateTimeProvider,
                client: _networkSettings.SocketFactory.Create(ipV4Address),
                settings: _networkSettings,
                arrayPool: ArrayPool<byte>.Shared,
                packetsPool: packetsPool);
        }
    }
}