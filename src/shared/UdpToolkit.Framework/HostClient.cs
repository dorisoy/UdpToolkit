namespace UdpToolkit.Framework
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Sockets;

    public sealed class HostClient : IHostClient
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly HostClientSettingsInternal _hostClientSettingsInternal;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUdpToolkitLogger _logger;
        private readonly IUdpClient _udpClient;
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;

        private DateTimeOffset _startConnect;
        private bool _disposed = false;

        public HostClient(
            HostClientSettingsInternal hostClientSettingsInternal,
            IDateTimeProvider dateTimeProvider,
            IUdpToolkitLogger logger,
            CancellationTokenSource cancellationTokenSource,
            IUdpClient udpClient,
            IQueueDispatcher<OutPacket> outQueueDispatcher)
        {
            _hostClientSettingsInternal = hostClientSettingsInternal;
            _cancellationTokenSource = cancellationTokenSource;
            _udpClient = udpClient;
            _outQueueDispatcher = outQueueDispatcher;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;

            OnConnectionTimeout += () =>
            {
                IsConnected = false;
            };
        }

        ~HostClient()
        {
            Dispose(false);
        }

        public event Action<IpV4Address, Guid> OnDisconnected;

        public event Action<IpV4Address, Guid> OnConnected;

        public event Action OnConnectionTimeout;

        public event Action<double> OnRttReceived;

        private bool IsConnected { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Connect()
        {
            _startConnect = _dateTimeProvider.GetUtcNow();
            _udpClient.Connect(_hostClientSettingsInternal.ServerIpV4);

            var token = _cancellationTokenSource.Token;

            Task.Factory.StartNew(
                function: () => StartHeartbeat(token),
                cancellationToken: token,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Current);
        }

        public void Connect(
            string host,
            int port)
        {
            var destination = new IpV4Address(IPAddress.Parse(host).ToInt(), (ushort)port);

            _udpClient.Connect(destination);
        }

        public void Disconnect()
        {
            _udpClient.Disconnect(_hostClientSettingsInternal.ServerIpV4);
        }

        public void Disconnect(
            string host,
            int port)
        {
            var from = new IpV4Address(IPAddress.Parse(host).ToInt(), (ushort)port);

            _udpClient.Disconnect(from);
        }

        public void Send<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId)
        {
            SendInternal(
                @event: @event,
                destination: destination,
                channelId: channelId);
        }

        public void Send<TEvent>(
            TEvent @event,
            byte channelId)
        {
            SendInternal(
                @event: @event,
                destination: _hostClientSettingsInternal.ServerIpV4,
                channelId: channelId);
        }

        internal void Connected(
            IpV4Address ipV4,
            Guid connectionId)
        {
            IsConnected = true;
            OnConnected?.Invoke(ipV4, connectionId);
        }

        internal void Disconnected(
            IpV4Address ipV4,
            Guid connectionId)
        {
            IsConnected = false;
            OnDisconnected?.Invoke(ipV4, connectionId);
        }

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
        {
            if (_udpClient.IsConnected(out var connectionId))
            {
                _outQueueDispatcher
                    .Dispatch(connectionId)
                    .Produce(new OutPacket(
                        connectionId: connectionId,
                        channelId: channelId,
                        @event: @event,
                        ipV4Address: destination));
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
