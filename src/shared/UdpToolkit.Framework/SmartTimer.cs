namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// Internal wrapper for using a timer.
    /// </summary>
    internal sealed class SmartTimer : IDisposable
    {
        private readonly TimeSpan _ttl;
        private readonly DateTimeOffset _createdAt;
        private readonly Timer _timer;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartTimer"/> class.
        /// </summary>
        /// <param name="callback">Timer callback.</param>
        /// <param name="delay">Delay for first call.</param>
        /// <param name="frequency">Frequency of cals.</param>
        /// <param name="createdAt">Creation date.</param>
        /// <param name="ttl">Ttl for timer.</param>
        internal SmartTimer(
            TimerCallback callback,
            TimeSpan delay,
            TimeSpan frequency,
            DateTimeOffset createdAt,
            TimeSpan ttl)
        {
            _createdAt = createdAt;
            _ttl = ttl;
            _timer = new Timer(
                callback: callback,
                state: null,
                dueTime: delay,
                period: frequency);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SmartTimer"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~SmartTimer()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Checking expiration of timer.
        /// </summary>
        /// <param name="utcNow">Utc now.</param>
        /// <returns>
        /// true - timer expired
        /// false - timer alive.
        /// </returns>
        internal bool IsExpired(DateTimeOffset utcNow)
        {
            return utcNow - _createdAt > _ttl;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer?.Dispose();
            }

            _disposed = true;
        }
    }
}