namespace UdpToolkit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Serialization;

    public sealed class HostClient : IHostClient
    {
        private readonly int? _heartbeatDelayMs;
        private readonly TimeSpan _connectionTimeout;
        private readonly TaskFactory _taskFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConnection _clientConnection;
        private readonly ISerializer _serializer;
        private readonly IClientBroadcaster _clientBroadcaster;
        private readonly IUdpToolkitLogger _logger;

        private bool _disposed = false;

        private DateTimeOffset _startConnect;

        public HostClient(
            IConnection clientConnection,
            ISerializer serializer,
            int? heartbeatDelayMs,
            IClientBroadcaster clientBroadcaster,
            TimeSpan connectionTimeout,
            IDateTimeProvider dateTimeProvider,
            IUdpToolkitLogger logger,
            TaskFactory taskFactory,
            CancellationTokenSource cancellationTokenSource)
        {
            _clientConnection = clientConnection;
            _serializer = serializer;
            _heartbeatDelayMs = heartbeatDelayMs;
            _taskFactory = taskFactory;
            _cancellationTokenSource = cancellationTokenSource;
            _clientBroadcaster = clientBroadcaster;
            _connectionTimeout = connectionTimeout;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _taskFactory = taskFactory;
        }

        ~HostClient()
        {
            Dispose(false);
        }

        public event Action OnConnectionTimeout;

        public Guid ConnectionId => _clientConnection.ConnectionId;

        public bool IsConnected { get; internal set; }

        public TimeSpan Rtt => _clientConnection.GetRtt();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Connect()
        {
            _startConnect = _dateTimeProvider.UtcNow();

            SendInternal(
                @event: new Connect(ConnectionId),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Connect,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Connect>.Serialize);

            var token = _cancellationTokenSource.Token;

            _taskFactory.StartNew(
                function: () => StartHeartbeat(token),
                cancellationToken: token,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Current);
        }

        public void Disconnect()
        {
            SendInternal(
                @event: new Disconnect(connectionId: ConnectionId),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Disconnect,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Disconnect>.Serialize);
        }

        public void Send<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode)
        {
            SendInternal(
                packetType: PacketType.FromClient,
                @event: @event,
                hookId: hookId,
                udpMode: udpMode,
                serializer: _serializer.Serialize);
        }

        private void SendInternal<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode,
            PacketType packetType,
            Func<TEvent, byte[]> serializer)
        {
            _clientBroadcaster.Broadcast(
                serializer: () => serializer(@event),
                caller: ConnectionId,
                hookId: hookId,
                packetType: packetType,
                channelType: udpMode.Map());
        }

        private async Task StartHeartbeat(
            CancellationToken cancellationToken)
        {
            if (!_heartbeatDelayMs.HasValue)
            {
                return;
            }

            var heartbeatDelayMs = _heartbeatDelayMs.Value;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!IsConnected && _dateTimeProvider.UtcNow() - _startConnect > _connectionTimeout)
                {
                    OnConnectionTimeout?.Invoke();
                    return;
                }

                var @event = new Heartbeat();

                SendInternal(
                    @event: @event,
                    packetType: PacketType.Protocol,
                    hookId: (byte)ProtocolHookId.Heartbeat,
                    udpMode: UdpMode.ReliableUdp,
                    serializer: ProtocolEvent<Heartbeat>.Serialize);

                await Task.Delay(heartbeatDelayMs, cancellationToken).ConfigureAwait(false);
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
                _clientBroadcaster.Dispose();
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}
