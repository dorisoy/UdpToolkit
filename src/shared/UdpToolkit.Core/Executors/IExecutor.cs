namespace UdpToolkit.Core.Executors
{
    using System;
    using System.Threading;

    public interface IExecutor : IDisposable
    {
        void Execute(
            Action action,
            string opName,
            CancellationToken cancellationToken);
    }
}