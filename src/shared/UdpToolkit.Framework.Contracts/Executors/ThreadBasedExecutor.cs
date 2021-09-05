namespace UdpToolkit.Framework.Contracts.Executors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Executor implementation based on threads.
    /// </summary>
    public sealed class ThreadBasedExecutor : IExecutor
    {
        private readonly List<Thread> _threads = new List<Thread>();
        private bool _disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="ThreadBasedExecutor"/> class.
        /// </summary>
        ~ThreadBasedExecutor()
        {
            Dispose(false);
        }

        /// <summary>
        /// Raised when an exception is thrown while action executing.
        /// </summary>
        public event Action<Exception> OnException;

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Execute provided action.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="opName">Operation name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
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