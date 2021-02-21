namespace UdpToolkit
{
    using System;

    public class ScheduledCall<TEvent>
    {
        public ScheduledCall(
            short timerId,
            Action<Guid, TEvent> action,
            TimeSpan delay)
        {
            TimerId = timerId;
            Action = action;
            Delay = delay;
        }

        public short TimerId { get; }

        public Action<Guid, TEvent> Action { get; }

        public TimeSpan Delay { get; }
    }
}