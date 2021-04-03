namespace UdpToolkit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Serialization;

    public sealed class HostClient : IHostClient
    {
        private readonly int? _heartbeatDelayMs;
        private readonly int[] _inputPorts;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly TimeSpan _connectionTimeout;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConnection _hostConnection;
        private readonly ISerializer _serializer;
        private readonly IClientBroadcaster _clientBroadcaster;

        private DateTimeOffset _startConnect;

        public HostClient(
            IConnection hostConnection,
            ISerializer serializer,
            int? heartbeatDelayMs,
            int[] inputPorts,
            CancellationTokenSource cancellationTokenSource,
            IClientBroadcaster clientBroadcaster,
            TimeSpan connectionTimeout,
            IDateTimeProvider dateTimeProvider)
        {
            _hostConnection = hostConnection;
            _serializer = serializer;
            _heartbeatDelayMs = heartbeatDelayMs;
            _inputPorts = inputPorts;
            _cancellationTokenSource = cancellationTokenSource;
            _clientBroadcaster = clientBroadcaster;
            _connectionTimeout = connectionTimeout;
            _dateTimeProvider = dateTimeProvider;
        }

        public event Action OnConnectionTimeout;

        public Guid ConnectionId => _hostConnection.ConnectionId;

        public bool IsConnected { get; internal set; }

        public TimeSpan Rtt => _hostConnection.GetRtt();

        public void Connect()
        {
            _startConnect = _dateTimeProvider.UtcNow();

            SendInternal(
                @event: new Connect(ConnectionId, _inputPorts),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Connect,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Connect>.Serialize);

            Task.Run(() => StartHeartbeat(_cancellationTokenSource.Token));
        }

        public void Disconnect()
        {
            _cancellationTokenSource.Cancel();

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
                broadcastMode: BroadcastMode.Server,
                channelType: udpMode.Map());
        }

        private async Task StartHeartbeat(
            CancellationToken token)
        {
            if (!_heartbeatDelayMs.HasValue)
            {
                return;
            }

            var heartbeatDelayMs = _heartbeatDelayMs.Value;
            while (!token.IsCancellationRequested)
            {
                if (!IsConnected)
                {
                    if (_dateTimeProvider.UtcNow() - _startConnect > _connectionTimeout)
                    {
                        OnConnectionTimeout?.Invoke();
                        return;
                    }
                }

                var @event = new Heartbeat();

                SendInternal(
                    @event: @event,
                    packetType: PacketType.Protocol,
                    hookId: (byte)ProtocolHookId.Heartbeat,
                    udpMode: UdpMode.ReliableUdp,
                    serializer: ProtocolEvent<Heartbeat>.Serialize);

                await Task.Delay(heartbeatDelayMs, token).ConfigureAwait(false);
            }
        }
    }
}
