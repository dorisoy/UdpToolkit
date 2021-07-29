namespace UdpToolkit
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Sockets;
    using UdpToolkit.Serialization;
    using IpV4Address = UdpToolkit.Core.IpV4Address;

    public sealed class HostClient : IHostClient
    {
        private readonly int? _heartbeatDelayMs;
        private readonly TimeSpan _connectionTimeout;
        private readonly TaskFactory _taskFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConnection _clientConnection;
        private readonly ISerializer _serializer;
        private readonly IUdpToolkitLogger _logger;
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;

        private bool _disposed = false;

        private DateTimeOffset _startConnect;

        public HostClient(
            IConnection clientConnection,
            ISerializer serializer,
            int? heartbeatDelayMs,
            TimeSpan connectionTimeout,
            IDateTimeProvider dateTimeProvider,
            IUdpToolkitLogger logger,
            TaskFactory taskFactory,
            CancellationTokenSource cancellationTokenSource,
            IQueueDispatcher<OutPacket> outQueueDispatcher)
        {
            _clientConnection = clientConnection;
            _serializer = serializer;
            _heartbeatDelayMs = heartbeatDelayMs;
            _taskFactory = taskFactory;
            _cancellationTokenSource = cancellationTokenSource;
            _outQueueDispatcher = outQueueDispatcher;
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
                destination: _clientConnection.IpAddress,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Connect>.Serialize);

            var token = _cancellationTokenSource.Token;

            _taskFactory.StartNew(
                function: () => StartHeartbeat(token),
                cancellationToken: token,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Current);
        }

        public void ConnectToPeer(
            string host,
            int port)
        {
            SendInternal(
                @event: new ConnectToPeer(ConnectionId),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Connect2Peer,
                destination: new Network.Sockets.IpV4Address
                {
                    Address = IPAddress.Parse(host).ToInt(),
                    Port = (ushort)port,
                },
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<ConnectToPeer>.Serialize);
        }

        public void Disconnect()
        {
            SendInternal(
                @event: new Disconnect(connectionId: ConnectionId),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Disconnect,
                destination: _clientConnection.IpAddress,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Disconnect>.Serialize);
        }

        public void Send<TEvent>(
            TEvent @event,
            byte hookId,
            IpV4Address destination,
            UdpMode udpMode)
        {
            SendInternal(
                packetType: PacketType.FromClient,
                @event: @event,
                hookId: hookId,
                destination: new Network.Sockets.IpV4Address
                {
                    Address = destination.Host.ToInt(),
                    Port = destination.Port,
                },
                udpMode: udpMode,
                serializer: _serializer.Serialize);
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
                destination: _clientConnection.IpAddress,
                udpMode: udpMode,
                serializer: _serializer.Serialize);
        }

        private void SendInternal<TEvent>(
            TEvent @event,
            byte hookId,
            Network.Sockets.IpV4Address destination,
            UdpMode udpMode,
            PacketType packetType,
            Func<TEvent, byte[]> serializer)
        {
            var utcNow = _dateTimeProvider.UtcNow();
            _outQueueDispatcher
                .Dispatch(ConnectionId)
                .Produce(new OutPacket(
                     hookId: hookId,
                     channelType: udpMode.Map(),
                     packetType: packetType,
                     connectionId: ConnectionId,
                     serializer: () => serializer(@event),
                     createdAt: utcNow,
                     destination: destination));
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
                    destination: _clientConnection.IpAddress,
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
                _outQueueDispatcher.Dispose();
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}
