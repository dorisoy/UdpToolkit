namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;

    /// <summary>
    /// Scheduler, implementation for sending delayed packets.
    /// </summary>
    public sealed class Scheduler : IScheduler
    {
        private readonly IBroadcaster _broadcaster;
        private readonly IRoomManager _roomManager;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<TimerKey, Lazy<Timer>> _timers = new ConcurrentDictionary<TimerKey, Lazy<Timer>>();
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler"/> class.
        /// </summary>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="roomManager">Instance of room manager.</param>
        /// <param name="broadcaster">Instance of broadcaster.</param>
        public Scheduler(
            ILogger logger,
            IRoomManager roomManager,
            IBroadcaster broadcaster)
        {
            _logger = logger;
            _roomManager = roomManager;
            _broadcaster = broadcaster;
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
        public void Schedule<TInEvent>(
            TInEvent inEvent,
            Guid caller,
            TimerKey timerKey,
            TimeSpan dueTime,
            Action<Guid, TInEvent, IRoomManager, IBroadcaster> action)
        {
            var lazyTimer = _timers.GetOrAdd(
                key: timerKey,
                valueFactory: (key) => new Lazy<Timer>(() => new Timer(
                    callback: (state) =>
                    {
                        action.Invoke(caller, inEvent, _roomManager, _broadcaster);
                    },
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

                _roomManager.Dispose();
            }

            _disposed = true;
        }
    }
}
