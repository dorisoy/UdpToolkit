namespace UdpToolkit.Network.Queues
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;

    /// <summary>
    /// Abstraction for storing packets without acknowledges.
    /// </summary>
    internal interface IResendQueue
    {
        /// <summary>
        /// Add packet to queue.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="pendingPacket">Instance of pending packet.</param>
        void Add(
            Guid connectionId,
            PendingPacket pendingPacket);

        /// <summary>
        /// Get List of pending packets for specific connection.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <returns>List of pending packets.</returns>
        public List<PendingPacket> Get(
            Guid connectionId);
    }
}