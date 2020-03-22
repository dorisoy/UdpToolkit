namespace UdpToolkit.Network.Queues
{
    using System.Collections.Generic;

    public interface IAsyncQueue<TEvent>
    {
        IEnumerable<TEvent> Consume();

        void Produce(TEvent @event);
    }
}