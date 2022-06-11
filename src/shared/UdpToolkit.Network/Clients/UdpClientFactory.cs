namespace UdpToolkit.Network.Clients
{
    using System.Buffers;
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
        private readonly IConnectionPool _lazyConnectionPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClientFactory"/> class.
        /// </summary>
        /// <param name="networkSettings">Instance of UDP client settings.</param>
        /// <param name="connectionPool">Instance of connection pool.</param>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        public UdpClientFactory(
            INetworkSettings networkSettings,
            IConnectionPool connectionPool,
            IDateTimeProvider dateTimeProvider = null)
        {
            _networkSettings = networkSettings;
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
                initSize: _networkSettings.PacketsPoolSize);

            return new UdpClient(
                connectionPool: _lazyConnectionPool,
                networkEventReporter: this._networkSettings.NetworkEventReporter,
                dateTimeProvider: _dateTimeProvider,
                client: _networkSettings.SocketFactory.Create(ipV4Address),
                settings: _networkSettings,
                arrayPool: ArrayPool<byte>.Shared,
                packetsPool: packetsPool);
        }
    }
}