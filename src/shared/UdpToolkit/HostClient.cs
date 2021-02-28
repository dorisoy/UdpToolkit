namespace UdpToolkit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Serialization;

    public sealed class HostClient : IHostClient
    {
        private readonly TimeSpan _connectionTimeoutFromSettings;
        private readonly TimeSpan _resendPacketsTimeout;

        private readonly int? _heartbeatDelayMs;
        private readonly int[] _inputPorts;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConnection _hostConnection;
        private readonly IConnection _remoteHostConnection;
        private readonly ISerializer _serializer;
        private readonly IAsyncQueue<CallContext> _outputQueue;

        private Task _heartbeatTask;

        public HostClient(
            IConnection hostConnection,
            IConnection remoteHostConnection,
            ISerializer serializer,
            IDateTimeProvider dateTimeProvider,
            IAsyncQueue<CallContext> outputQueue,
            TimeSpan connectionTimeout,
            TimeSpan resendPacketsTimeout,
            int? heartbeatDelayMs,
            int[] inputPorts,
            CancellationTokenSource cancellationTokenSource)
        {
            _hostConnection = hostConnection;
            _serializer = serializer;
            _dateTimeProvider = dateTimeProvider;
            _connectionTimeoutFromSettings = connectionTimeout;
            _resendPacketsTimeout = resendPacketsTimeout;
            _heartbeatDelayMs = heartbeatDelayMs;
            _inputPorts = inputPorts;
            _cancellationTokenSource = cancellationTokenSource;
            _remoteHostConnection = remoteHostConnection;
            _outputQueue = outputQueue;
        }

        public Guid ConnectionId => _hostConnection.ConnectionId;

        public TimeSpan Rtt => _hostConnection.GetRtt();

        public bool IsConnected { get; internal set; }

        public bool Connect(
            TimeSpan? connectionTimeout = null)
        {
            var timeout = connectionTimeout ?? _connectionTimeoutFromSettings;

            SendInternal(
                resendTimeout: timeout,
                @event: new Connect(ConnectionId, _inputPorts),
                networkPacketType: NetworkPacketType.Protocol,
                broadcastMode: Core.BroadcastMode.Server,
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
                resendTimeout: connectionTimeout ?? _connectionTimeoutFromSettings,
                @event: new Connect(ConnectionId, _inputPorts),
                networkPacketType: NetworkPacketType.Protocol,
                broadcastMode: Core.BroadcastMode.Server,
                hookId: (byte)ProtocolHookId.Connect,
                udpMode: UdpMode.ReliableUdp,
                serializer: ProtocolEvent<Connect>.Serialize);

            _heartbeatTask = Task.Run(() => StartHeartbeat(_cancellationTokenSource.Token));
        }

        public bool Disconnect()
        {
            SendInternal(
                resendTimeout: _resendPacketsTimeout,
                @event: new Disconnect(peerId: ConnectionId),
                broadcastMode: Core.BroadcastMode.Server,
                networkPacketType: NetworkPacketType.Protocol,
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
                networkPacketType: NetworkPacketType.FromClient,
                broadcastMode: Core.BroadcastMode.Server,
                resendTimeout: _resendPacketsTimeout,
                @event: @event,
                hookId: hookId,
                udpMode: udpMode,
                serializer: _serializer.Serialize);
        }

        private void SendInternal<TEvent>(
            TEvent @event,
            TimeSpan resendTimeout,
            byte hookId,
            UdpMode udpMode,
            NetworkPacketType networkPacketType,
            BroadcastMode broadcastMode,
            Func<TEvent, byte[]> serializer)
        {
            var utcNow = _dateTimeProvider.UtcNow();
            _outputQueue.Produce(new CallContext(
                resendTimeout: resendTimeout,
                createdAt: utcNow,
                roomId: null,
                broadcastMode: broadcastMode,
                networkPacket: new NetworkPacket(
                    hookId: hookId,
                    channelType: udpMode.Map(),
                    networkPacketType: networkPacketType,
                    connectionId: ConnectionId,
                    serializer: () => serializer(@event),
                    createdAt: utcNow,
                    ipEndPoint: _remoteHostConnection.GetIp())));
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
                    resendTimeout: _resendPacketsTimeout,
                    @event: @event,
                    networkPacketType: NetworkPacketType.Protocol,
                    broadcastMode: Core.BroadcastMode.Server,
                    hookId: (byte)ProtocolHookId.Heartbeat,
                    udpMode: UdpMode.ReliableUdp,
                    serializer: ProtocolEvent<Heartbeat>.Serialize);

                await Task.Delay(heartbeatDelayMs, token).ConfigureAwait(false);
            }
        }
    }
}
