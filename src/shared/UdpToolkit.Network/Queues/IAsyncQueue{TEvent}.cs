namespace UdpToolkit.Network.Queues
{
    using System;
    using System.Collections.Generic;

    public interface IAsyncQueue<TEvent> : IDisposable
    {
        IEnumerable<TEvent> Consume();

        void Produce(TEvent @event);

        void Stop();
    }
}