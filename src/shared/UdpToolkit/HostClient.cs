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
        private readonly TimeSpan _connectionTimeoutFromSettings;
        private readonly TimeSpan _resendPacketsTimeout;

        private readonly int? _heartbeatDelayMs;
        private readonly int[] _inputPorts;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConnection _hostConnection;
        private readonly IConnection _remoteHostConnection;
        private readonly ISerializer _serializer;
        private readonly IBroadcaster _broadcaster;

        private Task _heartbeatTask;

        public HostClient(
            IConnection hostConnection,
            IConnection remoteHostConnection,
            ISerializer serializer,
            TimeSpan connectionTimeout,
            TimeSpan resendPacketsTimeout,
            int? heartbeatDelayMs,
            int[] inputPorts,
            CancellationTokenSource cancellationTokenSource,
            IBroadcaster broadcaster)
        {
            _hostConnection = hostConnection;
            _serializer = serializer;
            _connectionTimeoutFromSettings = connectionTimeout;
            _resendPacketsTimeout = resendPacketsTimeout;
            _heartbeatDelayMs = heartbeatDelayMs;
            _inputPorts = inputPorts;
            _cancellationTokenSource = cancellationTokenSource;
            _broadcaster = broadcaster;
            _remoteHostConnection = remoteHostConnection;
        }

        public Guid ConnectionId => _hostConnection.ConnectionId;

        public TimeSpan Rtt => _hostConnection.GetRtt();

        public bool IsConnected { get; internal set; }

        public bool Connect(
            TimeSpan? connectionTimeout = null)
        {
            var timeout = connectionTimeout ?? _connectionTimeoutFromSettings;

            SendInternal(
                @event: new Connect(ConnectionId, _inputPorts),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Connect,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Connect>.Serialize);

            _heartbeatTask = Task.Run(() => StartHeartbeat(_cancellationTokenSource.Token));

            return SpinWait.SpinUntil(() => IsConnected, TimeSpan.FromMilliseconds(timeout.TotalMilliseconds * 1.2));
        }

        public void ConnectAsync(
            TimeSpan? connectionTimeout = null)
        {
            SendInternal(
                @event: new Connect(ConnectionId, _inputPorts),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Connect,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Connect>.Serialize);

            _heartbeatTask = Task.Run(() => StartHeartbeat(_cancellationTokenSource.Token));
        }

        public bool Disconnect()
        {
            SendInternal(
                @event: new Disconnect(connectionId: ConnectionId),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Disconnect,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Disconnect>.Serialize);

            return SpinWait.SpinUntil(() => !IsConnected, TimeSpan.FromMilliseconds(_resendPacketsTimeout.TotalMilliseconds * 1.2));
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
            _broadcaster.Broadcast(
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
