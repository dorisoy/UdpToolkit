namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// HostClient.
    /// </summary>
    public sealed class HostClient : IHostClient
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly HostClientSettingsInternal _hostClientSettingsInternal;
        private readonly ConcurrentPool<OutPacket> _outPacketsPool;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger _logger;
        private readonly IUdpClient _udpClient;
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;

        private DateTimeOffset _startConnect;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostClient"/> class.
        /// </summary>
        /// <param name="hostClientSettingsInternal">Instance of internal host client settings.</param>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="cancellationTokenSource">Instance of cancellation token source.</param>
        /// <param name="udpClient">Instance of UDP client.</param>
        /// <param name="outQueueDispatcher">Instance of outQueueDispatcher.</param>
        /// <param name="outPacketsPool">Instance of out packets pool.</param>
        public HostClient(
            HostClientSettingsInternal hostClientSettingsInternal,
            IDateTimeProvider dateTimeProvider,
            ILogger logger,
            CancellationTokenSource cancellationTokenSource,
            IUdpClient udpClient,
            IQueueDispatcher<OutPacket> outQueueDispatcher,
            ConcurrentPool<OutPacket> outPacketsPool)
        {
            _hostClientSettingsInternal = hostClientSettingsInternal;
            _cancellationTokenSource = cancellationTokenSource;
            _udpClient = udpClient;
            _outQueueDispatcher = outQueueDispatcher;
            _outPacketsPool = outPacketsPool;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;

            OnConnectionTimeout += () =>
            {
                IsConnected = false;
            };
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HostClient"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~HostClient()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public event Action<IpV4Address, Guid> OnDisconnected;

        /// <inheritdoc />
        public event Action<IpV4Address, Guid> OnConnected;

        /// <inheritdoc />
        public event Action OnConnectionTimeout;

        /// <inheritdoc />
        public event Action<double> OnRttReceived;

        private bool IsConnected { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Connect(
            Guid connectionId)
        {
            _startConnect = _dateTimeProvider.GetUtcNow();
            _udpClient.Connect(_hostClientSettingsInternal.ServerIpV4, connectionId);

            var token = _cancellationTokenSource.Token;

            Task.Factory.StartNew(
                function: () => StartHeartbeat(token),
                cancellationToken: token,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Current);
        }

        /// <inheritdoc />
        public void Connect(
            string host,
            int port,
            Guid connectionId)
        {
            var destination = new IpV4Address(IPAddress.Parse(host).ToInt(), (ushort)port);

            _udpClient.Connect(destination, connectionId);
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            _udpClient.Disconnect(_hostClientSettingsInternal.ServerIpV4);
        }

        /// <inheritdoc />
        public void Disconnect(
            string host,
            int port)
        {
            var from = new IpV4Address(IPAddress.Parse(host).ToInt(), (ushort)port);

            _udpClient.Disconnect(from);
        }

        /// <inheritdoc />
        public void Send<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId)
            where TEvent : class, IDisposable
        {
            SendInternal(
                @event: @event,
                destination: destination,
                channelId: channelId);
        }

        /// <inheritdoc />
        public void Send<TEvent>(
            TEvent @event,
            byte channelId)
            where TEvent : class, IDisposable
        {
            SendInternal(
                @event: @event,
                destination: _hostClientSettingsInternal.ServerIpV4,
                channelId: channelId);
        }

        /// <summary>
        /// Set state of host client to `Connected` (Internal use only).
        /// </summary>
        /// <param name="ipV4">Remote ip address.</param>
        /// <param name="connectionId">Connection identifier.</param>
        internal void Connected(
            IpV4Address ipV4,
            Guid connectionId)
        {
            Console.WriteLine("Connect");
            IsConnected = true;
            OnConnected?.Invoke(ipV4, connectionId);
        }

        /// <summary>
        /// Set state of host client to `Disconnected` (Internal use only).
        /// </summary>
        /// <param name="ipV4">Remote ip address.</param>
        /// <param name="connectionId">Connection identifier.</param>
        internal void Disconnected(
            IpV4Address ipV4,
            Guid connectionId)
        {
            IsConnected = false;
            OnDisconnected?.Invoke(ipV4, connectionId);
        }

        /// <summary>
        /// Update RTT time for host client (Internal use only).
        /// </summary>
        /// <param name="rtt">Round-trip time in ms.</param>
        internal void HeartbeatReceived(
            double rtt)
        {
            OnRttReceived?.Invoke(rtt);
        }

        private async Task StartHeartbeat(
            CancellationToken cancellationToken)
        {
            if (!_hostClientSettingsInternal.HeartbeatDelayMs.HasValue)
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!IsConnected && _dateTimeProvider.GetUtcNow() - _startConnect > _hostClientSettingsInternal.ConnectionTimeout)
                {
                    OnConnectionTimeout?.Invoke();
                    return;
                }

                _udpClient.Heartbeat(_hostClientSettingsInternal.ServerIpV4);

                await Task.Delay(_hostClientSettingsInternal.HeartbeatDelayMs.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendInternal<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId)
            where TEvent : class, IDisposable
        {
            if (_udpClient.IsConnected(out var connectionId))
            {
                var outPacket = _outPacketsPool.GetOrCreate();
                outPacket.Setup(
                    @event: @event,
                    ipV4Address: destination,
                    connectionId: connectionId,
                    channelId: channelId);

                _outQueueDispatcher
                    .Dispatch(connectionId)
                    .Produce(outPacket);
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
                _cancellationTokenSource.Cancel();
                _udpClient.Dispose();
            }

            _disposed = true;
        }
    }
}
