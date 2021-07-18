namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Concurrent;
    using UdpToolkit.Logging;

    public sealed class BlockingAsyncQueue<TEvent> : IAsyncQueue<TEvent>
    {
        private readonly Action<TEvent> _action;
        private readonly BlockingCollection<TEvent> _input;
        private readonly IUdpToolkitLogger _logger;

        private bool _disposed = false;

        public BlockingAsyncQueue(
            int boundedCapacity,
            Action<TEvent> action,
            IUdpToolkitLogger logger)
        {
            _action = action;
            _logger = logger;
            _input = new BlockingCollection<TEvent>(
                boundedCapacity: boundedCapacity,
                collection: new ConcurrentQueue<TEvent>());
        }

        ~BlockingAsyncQueue()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Produce(
            TEvent @event)
        {
            try
            {
                _input.Add(@event);
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        public void Consume()
        {
            try
            {
                foreach (var @event in _input.GetConsumingEnumerable())
                {
                    try
                    {
                        _action(@event);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Exception on receive task: {ex}");
                        _logger.Warning("Restart receiver...");
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _input.CompleteAdding();
                _input.Dispose();
            }

            _disposed = true;
        }
    }
}