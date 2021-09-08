namespace UdpToolkit.Framework.Contracts.Executors
{
    using System;
    using System.Threading;

    /// <summary>
    /// Abstraction for a run any actions in Threads, Tasks, Unity Jobs System e.t.c.
    /// </summary>
    public interface IExecutor : IDisposable
    {
        /// <summary>
        /// Raised when an exception is thrown while action executing.
        /// </summary>
        event Action<Exception> OnException;

        /// <summary>
        /// Execute provided action.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="opName">Operation name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        void Execute(
            Action action,
            string opName,
            CancellationToken cancellationToken);
    }
}