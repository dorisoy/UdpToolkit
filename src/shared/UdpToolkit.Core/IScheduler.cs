namespace UdpToolkit.Core
{
    using System;

    public interface IScheduler
    {
        void Schedule(
            ushort roomId,
            short timerId,
            int dueTimeMs,
            Action action);

        void Unschedule(
            ushort roomId,
            short timerId);
    }
}