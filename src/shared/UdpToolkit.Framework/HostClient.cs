namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using UdpToolkit.Framework.CodeGenerator.Contracts;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Packets;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// HostClient.
    /// </summary>
    public sealed class HostClient : IHostClient
    {
        private readonly IpV4Address _serverIpAddress;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IHostWorker _hostWorker;
        private readonly IUdpClient _udpClient;
        private readonly IQueueDispatcher<IOutNetworkPacket> _outQueueDispatcher;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostClient"/> class.
        /// </summary>
        /// <param name="serverIpAddress">Server ip address.</param>
        /// <param name="cancellationTokenSource">Instance of cancellation token source.</param>
        /// <param name="udpClient">Instance of UDP client.</param>
        /// <param name="outQueueDispatcher">Instance of outQueueDispatcher.</param>
        /// <param name="hostWorker">Instance of host worker.</param>
        public HostClient(
            IpV4Address serverIpAddress,
            CancellationTokenSource cancellationTokenSource,
            IUdpClient udpClient,
            IQueueDispatcher<IOutNetworkPacket> outQueueDispatcher,
            IHostWorker hostWorker)
        {
            _serverIpAddress = serverIpAddress;
            _cancellationTokenSource = cancellationTokenSource;
            _udpClient = udpClient;
            _outQueueDispatcher = outQueueDispatcher;
            _hostWorker = hostWorker;

            this._udpClient.OnPacketExpired += (pendingPacket) =>
            {
                if (pendingPacket.DataType == NetworkConsts.Connect)
                {
                    this.OnConnectionTimeout?.Invoke();
                }
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
            Guid connectionId,
            Guid routingKey)
        {
            _udpClient.Connect(_serverIpAddress, connectionId, routingKey);
        }

        /// <inheritdoc />
        public void Connect(
            string host,
            int port,
            Guid connectionId,
            Guid routingKey)
        {
            var destination = new IpV4Address(IpUtils.ToInt(host), (ushort)port);

            _udpClient.Connect(destination, connectionId, routingKey);
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            _udpClient.Disconnect(_serverIpAddress);
        }

        /// <inheritdoc />
        public void Disconnect(
            string host,
            int port)
        {
            var from = new IpV4Address(IpUtils.ToInt(host), (ushort)port);

            _udpClient.Disconnect(from);
        }

        /// <inheritdoc />
        public void Ping()
        {
            _udpClient.Ping(_serverIpAddress);
        }

        /// <inheritdoc />
        public void ResendPackets()
        {
            _udpClient.ResendPackets();
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
                destination: _serverIpAddress,
                channelId: channelId);
        }

        /// <inheritdoc />
        public void SendUnmanaged<TEvent>(
            TEvent @event,
            byte channelId)
            where TEvent : unmanaged
        {
            UnmanagedSendInternal(
                @event: @event,
                destination: _serverIpAddress,
                channelId: channelId);
        }

        /// <inheritdoc />
        public void SendUnmanaged<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId)
            where TEvent : unmanaged
        {
            UnmanagedSendInternal(
                @event: @event,
                destination: destination,
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
        internal void RttReceived(
            double rtt)
        {
            OnRttReceived?.Invoke(rtt);
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
                if (!_hostWorker.TryGetSubscriptionId(typeof(TEvent), out var subscriptionId))
                {
                    return;
                }

                var outPacket = ObjectsPool<ClientOutNetworkPacket<TEvent>>.GetOrCreate();
                var bufferWriter = ObjectsPool<BufferWriter<byte>>.GetOrCreate();

                outPacket.Setup(
                    bufferWriter: bufferWriter,
                    @event: @event,
                    dataType: subscriptionId,
                    ipV4Address: destination,
                    connectionId: connectionId,
                    channelId: channelId);

                _outQueueDispatcher
                    .Dispatch(connectionId)
                    .Produce(outPacket);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnmanagedSendInternal<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId)
        where TEvent : unmanaged
        {
            if (_udpClient.IsConnected(out var connectionId))
            {
                if (!_hostWorker.TryGetSubscriptionId(typeof(TEvent), out var subscriptionId))
                {
                    return;
                }

                var outPacket = ObjectsPool<ClientOutUnmanagedNetworkPacket<TEvent>>.GetOrCreate();
                var bufferWriter = ObjectsPool<BufferWriter<byte>>.GetOrCreate();

                outPacket.Setup(
                    bufferWriter: bufferWriter,
                    @event: @event,
                    dataType: subscriptionId,
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
