namespace UdpToolkit.Framework.Contracts
{
    using System;

    public interface IQueueDispatcher<TEvent> : IDisposable
    {
        int Count { get; }

        IAsyncQueue<TEvent> this[int index] { get; set; }

        IAsyncQueue<TEvent> Dispatch(
            Guid connectionId);
    }
}