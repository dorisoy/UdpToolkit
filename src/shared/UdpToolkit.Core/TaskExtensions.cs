namespace UdpToolkit.Core
{
    using System;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static async Task RestartJobOnFailAsync(
            this Task task,
            Func<Task> job,
            Action<Exception> logger)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger(ex);

                await Task.Run(job).RestartJobOnFailAsync(job, logger).ConfigureAwait(false);
            }
        }
    }
}