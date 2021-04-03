namespace UdpToolkit.Network.Queues
{
    using System;

    public interface IAsyncQueue<TEvent> : IDisposable
    {
        void Produce(
            TEvent @event);

        void Stop();

        void Consume();
    }
}