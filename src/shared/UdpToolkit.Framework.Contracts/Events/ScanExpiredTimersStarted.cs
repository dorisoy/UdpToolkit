namespace UdpToolkit.Framework.Contracts.Events
{
    using System;

    /// <summary>
    /// Raised when scan expired timers started.
    /// </summary>
    public readonly struct ScanExpiredTimersStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanExpiredTimersStarted"/> struct.
        /// </summary>
        /// <param name="startedAt">Start date time.</param>
        public ScanExpiredTimersStarted(
            DateTimeOffset startedAt)
        {
            StartedAt = startedAt;
        }

        /// <summary>
        /// Gets timers count.
        /// </summary>
        public DateTimeOffset StartedAt { get; }
    }
}