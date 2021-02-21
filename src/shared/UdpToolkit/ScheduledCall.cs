namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;

    public class ScheduledCall<TEvent>
    {
        public ScheduledCall(
            short timerId,
            Action<Guid, TEvent, IRoomManager> action,
            TimeSpan delay)
        {
            TimerId = timerId;
            Action = action;
            Delay = delay;
        }

        public short TimerId { get; }

        public Action<Guid, TEvent, IRoomManager> Action { get; }

        public TimeSpan Delay { get; }
    }
}