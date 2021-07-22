namespace UdpToolkit.Core.Executors
{
    using System.Threading.Tasks;
    using UdpToolkit.Logging;

    public static class ExecutorFactory
    {
        public static IExecutor Create(
            ExecutorType executorType,
            IUdpToolkitLogger logger,
            TaskFactory taskFactory)
        {
            return executorType == ExecutorType.TaskBasedExecutor
                ? new TaskBasedExecutor(logger, taskFactory)
                : new ThreadBasedExecutor(logger);
        }
    }
}