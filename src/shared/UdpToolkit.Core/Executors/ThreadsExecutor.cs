namespace UdpToolkit.Core.Executors
{
    using System;
    using System.Threading;
    using UdpToolkit.Logging;

    public sealed class ThreadsExecutor : IExecutor
    {
        private readonly IUdpToolkitLogger _logger;

        public ThreadsExecutor(IUdpToolkitLogger logger)
        {
            _logger = logger;
        }

        public void Execute(
            Action action,
            bool restartOnFail,
            string opName)
        {
            _logger.Debug($"Run action {opName} on thread based executor");
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Exception {ex} on execute action: {opName}");
                    if (restartOnFail)
                    {
                        _logger.Warning($"Restart action {opName}...");
                        Execute(action, true, opName);
                    }
                }
            });
            thread.Start();
        }
    }
}