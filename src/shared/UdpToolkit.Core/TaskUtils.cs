namespace UdpToolkit.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class TaskUtils
    {
        public static async Task RestartOnFail(
            Func<Task> job,
            Action<Exception> logger,
            CancellationToken token = default)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            while (!token.IsCancellationRequested)
            {
                var task = Task.Run(job, token)
                        .ContinueWith(
                            continuationAction: (t) => logger(t.Exception),
                            continuationOptions: TaskContinuationOptions.OnlyOnFaulted);

                await task.ConfigureAwait(false);
            }
        }
    }
}