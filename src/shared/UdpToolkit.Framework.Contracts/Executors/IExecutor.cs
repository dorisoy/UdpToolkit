namespace UdpToolkit.Framework.Contracts.Executors
{
    using System;
    using System.Threading;

    public interface IExecutor : IDisposable
    {
        event Action<Exception> OnException;

        void Execute(
            Action action,
            string opName,
            CancellationToken cancellationToken);
    }
}