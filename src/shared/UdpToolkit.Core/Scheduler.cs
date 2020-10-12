namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public sealed class Scheduler : IScheduler
    {
        private readonly ConcurrentDictionary<TimerKey, Timer> _timers = new ConcurrentDictionary<TimerKey, Timer>();

        public void Schedule(
            ushort roomId,
            short timerId,
            int dueTimeMs,
            Action action)
        {
            _timers.GetOrAdd(
                key: new TimerKey(
                    roomId: roomId,
                    timerId: timerId),
                valueFactory: (key) => new Timer(
                    callback: (state) => action(),
                    state: null,
                    dueTime: TimeSpan.FromMilliseconds(dueTimeMs),
                    period: TimeSpan.FromMilliseconds(Timeout.Infinite)));
        }

        public void Unschedule(
            ushort roomId,
            short timerId)
        {
            var success = _timers
                .TryRemove(
                    key: new TimerKey(
                        roomId: roomId,
                        timerId: timerId),
                    value: out var timer);

            if (success)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }
        }
    }
}