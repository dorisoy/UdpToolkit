namespace UdpToolkit
{
    using System;

    public class ScheduledCall<TEvent>
    {
        public ScheduledCall(
            short timerId,
            Action<TEvent> action,
            TimeSpan delay)
        {
            TimerId = timerId;
            Action = action;
            Delay = delay;
        }

        public short TimerId { get; }

        public Action<TEvent> Action { get; }

        public TimeSpan Delay { get; }
    }
}