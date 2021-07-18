namespace UdpToolkit.Core.Executors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;

    public sealed class TasksExecutor : IExecutor
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public TasksExecutor(
            IUdpToolkitLogger logger)
        {
            _logger = logger;
        }

        public void Execute(
            Action action,
            string opName)
        {
            _logger.Debug($"Run action {opName} on task based executor");

            Task.Run(
                action: () =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Exception {ex} on execute action: {opName}");
                    }
                },
                cancellationToken: _cts.Token);
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}