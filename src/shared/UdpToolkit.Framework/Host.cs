namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Contracts.Clients;

    /// <summary>
    /// Host.
    /// </summary>
    public sealed class Host : IHost
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IExecutor _executor;
        private readonly ILogger _logger;
        private readonly IHostClient _hostClient;
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;
        private readonly IQueueDispatcher<InPacket> _inQueueDispatcher;
        private readonly IUdpClient[] _udpClients;
        private readonly IList<IDisposable> _toDispose;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host"/> class.
        /// </summary>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="hostClient">Instance of host client.</param>
        /// <param name="outQueueDispatcher">Instance of outQueueDispatcher.</param>
        /// <param name="inQueueDispatcher">Instance of inQueueDispatcher.</param>
        /// <param name="udpClients">Array of udp-clients.</param>
        /// <param name="executor">Instance of executor.</param>
        /// <param name="toDispose">Array of host resources for disposing.</param>
        /// <param name="serviceProvider">Providing internal services.</param>
        /// <param name="cancellationTokenSource">Instance of cancellation token source.</param>
        public Host(
            ILogger logger,
            IHostClient hostClient,
            IQueueDispatcher<OutPacket> outQueueDispatcher,
            IQueueDispatcher<InPacket> inQueueDispatcher,
            IUdpClient[] udpClients,
            IExecutor executor,
            IList<IDisposable> toDispose,
            Contracts.IServiceProvider serviceProvider,
            CancellationTokenSource cancellationTokenSource)
        {
            _logger = logger;
            _hostClient = hostClient;
            _outQueueDispatcher = outQueueDispatcher;
            _inQueueDispatcher = inQueueDispatcher;
            _udpClients = udpClients;
            _executor = executor;
            _toDispose = toDispose;
            _cancellationTokenSource = cancellationTokenSource;
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Host"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~Host()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public Contracts.IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public IHostClient HostClient => _hostClient;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Run()
        {
            var token = _cancellationTokenSource.Token;
            for (var i = 0; i < _udpClients.Length; i++)
            {
                var index = i;

                _executor.Execute(
                    action: () => _udpClients[index].StartReceive(token),
                    opName: $"Receiver_{index}",
                    cancellationToken: token);
            }

            for (var i = 0; i < _inQueueDispatcher.Count; i++)
            {
                var index = i;

                _executor.Execute(
                    action: _inQueueDispatcher[index].Consume,
                    opName: $"Worker_{index}",
                    cancellationToken: token);
            }

            for (var i = 0; i < _outQueueDispatcher.Count; i++)
            {
                var index = i;

                _executor.Execute(
                    action: _outQueueDispatcher[index].Consume,
                    opName: $"Sender_{index}",
                    cancellationToken: token);
            }

            _logger.Information($"[UdpToolkit.Framework] {nameof(Host)} running...");
        }

        /// <inheritdoc />
        public void On<TEvent>(
            Subscription<TEvent> subscription)
        {
            SubscriptionStorage<TEvent>.Subscribe(subscription);
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
                for (var i = 0; i < _toDispose.Count; i++)
                {
                    _toDispose[i].Dispose();
                }
            }

            _disposed = true;
        }
    }
}
