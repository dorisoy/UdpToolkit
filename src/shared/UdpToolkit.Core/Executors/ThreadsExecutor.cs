namespace UdpToolkit.Core.Executors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UdpToolkit.Logging;

    public sealed class ThreadsExecutor : IExecutor
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly List<Thread> _threads = new List<Thread>();

        public ThreadsExecutor(
            IUdpToolkitLogger logger)
        {
            _logger = logger;
        }

        public void Execute(
            Action action,
            string opName)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Exception {ex} on execute action: {opName}");
                }
            });
            thread.Name = opName;

            _logger.Debug($"Run {opName} on thread based executor, threadId - {thread.ManagedThreadId}, {thread.Name}");

            _threads.Add(thread);

            thread.Start();
        }

        public void Dispose()
        {
            for (int i = 0; i < _threads.Count; i++)
            {
                _threads[i].Join();
            }
        }
    }
}