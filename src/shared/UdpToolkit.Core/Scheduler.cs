namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public sealed class Scheduler : IScheduler
    {
        private readonly ConcurrentDictionary<TimerKey, Lazy<Timer>> _timers = new ConcurrentDictionary<TimerKey, Lazy<Timer>>();
        private readonly ConcurrentDictionary<Guid, Lazy<Timer>> _timers2 = new ConcurrentDictionary<Guid, Lazy<Timer>>();

        public void Schedule(
            int roomId,
            short timerId,
            TimeSpan dueTime,
            Action action)
        {
            var lazyTimer = _timers.GetOrAdd(
                key: new TimerKey(
                    roomId: roomId,
                    timerId: timerId),
                valueFactory: (key) => new Lazy<Timer>(() => new Timer(
                    callback: (state) => action(),
                    state: null,
                    dueTime: dueTime,
                    period: TimeSpan.FromMilliseconds(Timeout.Infinite))));

            _ = lazyTimer.Value;
        }

        public Timer Schedule(
            Guid key,
            int dueTimeMs,
            Action action)
        {
            var lazyTimer = _timers2.GetOrAdd(
                key: key,
                valueFactory: (peerId) => new Lazy<Timer>(() => new Timer(
                    callback: (state) => action(),
                    state: null,
                    dueTime: TimeSpan.FromMilliseconds(dueTimeMs),
                    period: TimeSpan.FromMilliseconds(dueTimeMs))));

            return lazyTimer.Value;
        }
    }
}