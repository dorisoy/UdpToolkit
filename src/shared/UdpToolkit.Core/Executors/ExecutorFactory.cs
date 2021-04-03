namespace UdpToolkit.Core.Executors
{
    using UdpToolkit.Logging;

    public static class ExecutorFactory
    {
        public static IExecutor Create(
            ExecutorType executorType,
            IUdpToolkitLogger logger)
        {
            return executorType == ExecutorType.TaskBasedExecutor
                ? new TasksExecutor(logger)
                : new ThreadsExecutor(logger);
        }
    }
}