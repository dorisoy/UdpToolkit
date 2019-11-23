using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UdpToolkit.Network
{
    public sealed class AsyncQueue<TItem>
    {
        private readonly BlockingCollection<TItem> _input;

        public AsyncQueue(int boundedCapacity)
        {
            _input = new BlockingCollection<TItem>(
                boundedCapacity: boundedCapacity, 
                collection: new ConcurrentQueue<TItem>());
        }
        
        public void Publish(TItem @event)
        {
            _input.Add(@event);
        }

        public IEnumerable<TItem> Consume()
        {
            return _input.GetConsumingEnumerable();
        }
    }
}

