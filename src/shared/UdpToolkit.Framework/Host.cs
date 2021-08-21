namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Executors;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Contracts.Clients;

    public sealed class Host : IHost
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IExecutor _executor;
        private readonly IUdpToolkitLogger _logger;
        private readonly IHostClient _hostClient;
        private readonly IQueueDispatcher<OutPacket> _hostOutQueueDispatcher;
        private readonly IQueueDispatcher<InPacket> _inQueueDispatcher;
        private readonly IUdpClient[] _udpClients;
        private readonly IList<IDisposable> _toDispose;

        private bool _disposed = false;

        public Host(
            IUdpToolkitLogger logger,
            IHostClient hostClient,
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher,
            IQueueDispatcher<InPacket> inQueueDispatcher,
            IUdpClient[] udpClients,
            IExecutor executor,
            IList<IDisposable> toDispose,
            CancellationTokenSource cancellationTokenSource)
        {
            _logger = logger;
            _hostClient = hostClient;
            _hostOutQueueDispatcher = hostOutQueueDispatcher;
            _inQueueDispatcher = inQueueDispatcher;
            _udpClients = udpClients;
            _executor = executor;
            _toDispose = toDispose;
            _cancellationTokenSource = cancellationTokenSource;
        }

        ~Host()
        {
            Dispose(false);
        }

        public IHostClient HostClient => _hostClient;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

            for (var i = 0; i < _hostOutQueueDispatcher.Count; i++)
            {
                var index = i;

                _executor.Execute(
                    action: _hostOutQueueDispatcher[index].Consume,
                    opName: $"Sender_{index}",
                    cancellationToken: token);
            }

            _logger.Information($"[UdpToolkit.Framework] {nameof(Host)} running...");
        }

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
