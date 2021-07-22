namespace UdpToolkit.Core.Executors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;

    public sealed class TaskBasedExecutor : IExecutor
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly TaskFactory _taskFactory;
        private bool _disposed;

        public TaskBasedExecutor(
            IUdpToolkitLogger logger,
            TaskFactory taskFactory)
        {
            _logger = logger;
            _taskFactory = taskFactory;
        }

        ~TaskBasedExecutor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Execute(
            Action action,
            string opName,
            CancellationToken cancellationToken)
        {
            _logger.Debug($"Run action {opName} on task based executor");

            _taskFactory.StartNew(
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

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}