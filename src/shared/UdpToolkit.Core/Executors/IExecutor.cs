namespace UdpToolkit.Core.Executors
{
    using System;

    public interface IExecutor : IDisposable
    {
        void Execute(
            Action action,
            string opName);
    }
}