namespace UdpToolkit.Core
{
    using System;

    public interface IAsyncQueue<in TEvent> : IDisposable
    {
        void Produce(
            TEvent @event);

        void Consume();
    }
}