namespace UdpToolkit.Core.Executors
{
    using System;

    public interface IExecutor
    {
        void Execute(
            Action action,
            bool restartOnFail,
            string opName);
    }
}