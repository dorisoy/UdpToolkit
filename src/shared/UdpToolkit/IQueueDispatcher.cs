namespace UdpToolkit
{
    using System;
    using UdpToolkit.Network.Queues;

    public interface IQueueDispatcher<TEvent> : IDisposable
    {
        IAsyncQueue<TEvent> Dispatch(
            Guid connectionId);

        void RunAll(string description);

        void StopAll();
    }
}