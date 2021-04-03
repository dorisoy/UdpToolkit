namespace UdpToolkit.Network.Queues
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;

    public sealed class BlockingAsyncQueue<TEvent> : IAsyncQueue<TEvent>
    {
        private readonly Action<TEvent> _action;
        private readonly BlockingCollection<TEvent> _input;
        private readonly IUdpToolkitLogger _logger;

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

        public void Produce(
            TEvent @event)
        {
            _input.Add(@event);
        }

        public void Stop()
        {
            _input.CompleteAdding();
        }

        public void Dispose()
        {
            _input.Dispose();
        }

        public void Consume()
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
    }
}