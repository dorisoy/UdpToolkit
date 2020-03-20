using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UdpToolkit.Network.Queues
{
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
            _input.Add(@event);
        }

        public IEnumerable<TEvent> Consume()
        {
            return _input.GetConsumingEnumerable();
        }
    }
}