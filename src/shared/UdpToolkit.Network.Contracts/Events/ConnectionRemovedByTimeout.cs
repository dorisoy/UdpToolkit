namespace UdpToolkit.Network.Contracts.Events
{
    using System;

    /// <summary>
    /// Raised when connection removed by timeout.
    /// </summary>
    public readonly struct ConnectionRemovedByTimeout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRemovedByTimeout"/> struct.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="idleTime">Idle time.</param>
        public ConnectionRemovedByTimeout(
            Guid connectionId,
            TimeSpan idleTime)
        {
            ConnectionId = connectionId;
            IdleTime = idleTime;
        }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        public Guid ConnectionId { get; }

        /// <summary>
        /// Gets connection idle time.
        /// </summary>
        public TimeSpan IdleTime { get; }
    }
}