namespace UdpToolkit.Framework.Contracts.Executors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class ThreadBasedExecutor : IExecutor
    {
        private readonly List<Thread> _threads = new List<Thread>();
        private bool _disposed;

        ~ThreadBasedExecutor()
        {
            Dispose(false);
        }

        public event Action<Exception> OnException;

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
                    OnException?.Invoke(ex);
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

            _disposed = true;
        }
    }
}