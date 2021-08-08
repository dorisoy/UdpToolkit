namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;

    public interface IQueueDispatcher<TEvent> : IDisposable
    {
        int Count { get; }

        IAsyncQueue<TEvent> this[int index] { get; set; }

        IAsyncQueue<TEvent> Dispatch(
            Guid connectionId);
    }
}