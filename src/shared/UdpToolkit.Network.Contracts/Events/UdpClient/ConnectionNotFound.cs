namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using System;

    /// <summary>
    /// Raised when connection not found.
    /// </summary>
    public readonly struct ConnectionNotFound
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionNotFound"/> struct.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        public ConnectionNotFound(
            Guid connectionId)
        {
            ConnectionId = connectionId;
        }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        public Guid ConnectionId { get; }
    }
}