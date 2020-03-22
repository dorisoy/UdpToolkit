namespace UdpToolkit.Framework
{
    using System;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        internal static async Task RestartJobOnFailAsync(
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