namespace UdpToolkit
{
    using System;

    public interface IJobDispatcher
    {
        void QueueWorkItem(Action action);
    }
}