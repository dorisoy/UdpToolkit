namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Framework.Contracts;

    /// <summary>
    /// Executor implementation based on tasks.
    /// </summary>
    public sealed class TaskBasedExecutor : IExecutor
    {
        private bool _disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="TaskBasedExecutor"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~TaskBasedExecutor()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public event Action<Exception> OnException;

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Execute(
            Action action,
            string opName,
            CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(
                action: () =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(ex);
                    }
                },
                cancellationToken: cancellationToken,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Current);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // nothing to do
            }

            _disposed = true;
        }
    }
}