namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;

    public sealed class QueueDispatcher<TEvent> : IQueueDispatcher<TEvent>
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly IAsyncQueue<TEvent>[] _queues;

        private bool _disposed = false;

        public QueueDispatcher(
            IAsyncQueue<TEvent>[] queues,
            IUdpToolkitLogger logger)
        {
            _queues = queues;
            _logger = logger;
            Count = _queues.Length;
        }

        ~QueueDispatcher()
        {
            Dispose(false);
        }

        public int Count { get; }

        public IAsyncQueue<TEvent> this[int index]
        {
            get => _queues[index];
            set => _queues[index] = value;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IAsyncQueue<TEvent> Dispatch(Guid connectionId)
        {
            return _queues[MurMurHash.Hash3_x86_32(connectionId) % _queues.Length];
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                for (int i = 0; i < _queues.Length; i++)
                {
                    _queues[i].Dispose();
                }
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}