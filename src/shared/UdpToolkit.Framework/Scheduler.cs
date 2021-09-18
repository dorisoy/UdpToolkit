namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;

    /// <inheritdoc />
    public sealed class Scheduler : IScheduler
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _groupTtl;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<TimerKey, Lazy<SmartTimer>> _timers = new ConcurrentDictionary<TimerKey, Lazy<SmartTimer>>();
        private readonly Timer _housekeeper;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler"/> class.
        /// </summary>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="dateTimeProvider">Instance of dateTimeProvider.</param>
        /// <param name="cleanupFrequency">Cleanup frequency for housekeeper.</param>
        /// <param name="groupTtl">Group ttl.</param>
        public Scheduler(
            ILogger logger,
            IDateTimeProvider dateTimeProvider,
            TimeSpan cleanupFrequency,
            TimeSpan groupTtl)
        {
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _groupTtl = groupTtl;

            _housekeeper = new Timer(
                callback: CleanupExpiredTimers,
                state: null,
                dueTime: TimeSpan.FromSeconds(10),
                period: cleanupFrequency);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Scheduler"/> class.
        /// </summary>
        ~Scheduler()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Schedule(
            TimerKey timerKey,
            TimeSpan delay,
            TimeSpan frequency,
            TimeSpan? ttl,
            Action action)
        {
            var lazyTimer = _timers.GetOrAdd(
                key: timerKey,
                valueFactory: (key) => new Lazy<SmartTimer>(() => new SmartTimer(
                    ttl: ttl ?? _groupTtl,
                    createdAt: _dateTimeProvider.GetUtcNow(),
                    callback: (_) => action(),
                    delay: delay,
                    frequency: frequency)));

            _ = lazyTimer.Value;
        }

        private void CleanupExpiredTimers(
            object state)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[UdpToolkit.Framework] Cleanup expired timers");
            }

            for (int i = 0; i < _timers.Count; i++)
            {
                var pair = _timers.ElementAt(i);
                var timer = pair.Value.Value;
                if (timer.IsExpired(_dateTimeProvider.GetUtcNow()))
                {
                    timer.Dispose();

                    if (_timers.TryRemove(pair.Key, out _) && _logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"[UdpToolkit.Framework] Timer removed {pair.Key}");
                    }
                }
            }
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

                _housekeeper.Dispose();
            }

            _disposed = true;
        }
    }
}
