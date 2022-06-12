namespace UdpToolkit.Network.Contracts.Events
{
    using System;

    /// <summary>
    /// Raised when inactive connection scan started.
    /// </summary>
    public readonly struct ScanInactiveConnectionsStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanInactiveConnectionsStarted"/> struct.
        /// </summary>
        /// <param name="startedAt">Connections count.</param>
        public ScanInactiveConnectionsStarted(
            DateTimeOffset startedAt)
        {
            StartedAt = startedAt;
        }

        /// <summary>
        /// Gets started at date time.
        /// </summary>
        public DateTimeOffset StartedAt { get; }
    }
}