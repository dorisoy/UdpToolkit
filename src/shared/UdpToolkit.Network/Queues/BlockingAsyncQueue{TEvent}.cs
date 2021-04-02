namespace UdpToolkit.Network.Queues
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public sealed class BlockingAsyncQueue<TEvent> : IAsyncQueue<TEvent>
    {
        private readonly BlockingCollection<TEvent> _input;

        public BlockingAsyncQueue(int boundedCapacity)
        {
            _input = new BlockingCollection<TEvent>(
                boundedCapacity: boundedCapacity,
                collection: new ConcurrentQueue<TEvent>());
        }

        public void Produce(TEvent @event)
        {
            _input.TryAdd(@event);
        }

        public void Stop()
        {
            _input.CompleteAdding();
        }

        public IEnumerable<TEvent> Consume()
        {
            return _input.GetConsumingEnumerable();
        }

        public void Dispose()
        {
            _input?.Dispose();
        }
    }
}