namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core.Executors;
    using UdpToolkit.Network.Queues;

    public sealed class QueueDispatcher<TEvent> : IQueueDispatcher<TEvent>
    {
        private readonly IAsyncQueue<TEvent>[] _queues;
        private readonly IExecutor _executor;

        public QueueDispatcher(
            IAsyncQueue<TEvent>[] queues,
            IExecutor executor)
        {
            _queues = queues;
            _executor = executor;
        }

        public IAsyncQueue<TEvent> Dispatch(Guid connectionId)
        {
            return _queues[MurMurHash.Hash3_x86_32(connectionId) % _queues.Length];
        }

        public void RunAll()
        {
            for (int i = 0; i < _queues.Length; i++)
            {
                var queue = _queues[i];
                _executor.Execute(
                    action: queue.Consume,
                    restartOnFail: true,
                    opName: $"Queue {nameof(TEvent)}");
            }
        }

        public void StopAll()
        {
            for (int i = 0; i < _queues.Length; i++)
            {
                _queues[i].Stop();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _queues.Length; i++)
            {
                _queues[i].Dispose();
            }
        }
    }
}