using System.Collections.Generic;

namespace UdpToolkit.Network.Queues
{
    public interface IAsyncQueue<TEvent>
    {
        IEnumerable<TEvent> Consume();
        void Produce(TEvent @event);
    }
}