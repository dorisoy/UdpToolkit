namespace UdpToolkit.Core.Executors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UdpToolkit.Logging;

    public sealed class ThreadBasedExecutor : IExecutor
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly List<Thread> _threads = new List<Thread>();
        private bool _disposed;

        public ThreadBasedExecutor(
            IUdpToolkitLogger logger)
        {
            _logger = logger;
        }

        ~ThreadBasedExecutor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Execute(
            Action action,
            string opName,
            CancellationToken cancellationToken)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Exception {ex} on execute action: {opName}");
                }
            });
            thread.IsBackground = true;
            thread.Name = opName;

            _threads.Add(thread);

            thread.Start();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                for (var i = 0; i < _threads.Count; i++)
                {
                     _threads[i].Join();
                }
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}