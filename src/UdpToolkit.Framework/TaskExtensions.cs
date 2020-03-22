using System;
using System.Threading.Tasks;

namespace UdpToolkit.Framework
{
    internal static class TaskExtensions
    {
        internal static async Task RestartJobOnFail(
            this Task task,
            Func<Task> job,
            Action<Exception> logger)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (job == null) throw new ArgumentNullException(nameof(job));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            try
            {
                await task;
            }
            catch (Exception ex)
            {
                logger(ex);
                
                await Task.Run(job).RestartJobOnFail(job, logger);
            }
        }
    }
}