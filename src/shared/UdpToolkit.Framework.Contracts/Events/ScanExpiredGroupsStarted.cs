namespace UdpToolkit.Framework.Contracts.Events
{
    using System;

    /// <summary>
    /// Raised when expired groups started.
    /// </summary>
    public readonly struct ScanExpiredGroupsStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanExpiredGroupsStarted"/> struct.
        /// </summary>
        /// <param name="startedAt">Start date time.</param>
        public ScanExpiredGroupsStarted(
            DateTimeOffset startedAt)
        {
            StartedAt = startedAt;
        }

        /// <summary>
        /// Gets groups count.
        /// </summary>
        public DateTimeOffset StartedAt { get; }
    }
}