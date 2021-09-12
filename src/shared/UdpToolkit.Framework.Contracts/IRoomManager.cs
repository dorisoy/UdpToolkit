namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for managing room connections.
    /// </summary>
    public interface IRoomManager : IDisposable
    {
        /// <summary>
        /// Join or create room.
        /// </summary>
        /// <param name="roomId">Room identifier.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="ipV4Address">Ip address.</param>
        void JoinOrCreate(
            Guid roomId,
            Guid connectionId,
            IpV4Address ipV4Address);

        /// <summary>
        /// Get existing room.
        /// </summary>
        /// <param name="roomId">Room identifier.</param>
        /// <returns>
        /// Room.
        /// </returns>
        Room GetRoom(
            Guid roomId);
    }
}