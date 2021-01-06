namespace UdpToolkit.Core
{
    using System;

    public interface IScheduler
    {
        void Schedule(
            int roomId,
            short timerId,
            TimeSpan dueTime,
            Action action);
    }
}