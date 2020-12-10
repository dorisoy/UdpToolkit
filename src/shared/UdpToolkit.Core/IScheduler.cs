namespace UdpToolkit.Core
{
    using System;

    public interface IScheduler
    {
        void Schedule(
            int roomId,
            short timerId,
            int dueTimeMs,
            Action action);
    }
}