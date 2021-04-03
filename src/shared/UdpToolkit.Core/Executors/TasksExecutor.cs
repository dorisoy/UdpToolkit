namespace UdpToolkit.Core.Executors
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;

    public sealed class TasksExecutor : IExecutor
    {
        private readonly IUdpToolkitLogger _logger;

        public TasksExecutor(
            IUdpToolkitLogger logger)
        {
            _logger = logger;
        }

        public void Execute(
            Action action,
            bool restartOnFail,
            string opName)
        {
            _logger.Debug($"Run action {opName} on task based executor");

            Task.Run(() =>
            {
                try
                {
                    action.Invoke();
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
        }
    }
}