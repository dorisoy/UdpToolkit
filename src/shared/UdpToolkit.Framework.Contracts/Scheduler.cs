namespace UdpToolkit.Framework.Contracts
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Logging;

    public sealed class Scheduler : IScheduler
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly ConcurrentDictionary<TimerKey, Lazy<Timer>> _timers = new ConcurrentDictionary<TimerKey, Lazy<Timer>>();
        private bool _disposed = false;

        public Scheduler(
            IUdpToolkitLogger logger)
        {
            _logger = logger;
        }

        ~Scheduler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                for (var i = 0; i < _timers.Count; i++)
                {
                    var timer = _timers.ElementAt(i).Value;
                    timer.Value.Dispose();
                }
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}