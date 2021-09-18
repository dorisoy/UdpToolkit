namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for managing groups of connections.
    /// </summary>
    public interface IGroupManager : IDisposable
    {
        /// <summary>
        /// Join or create group.
        /// </summary>
        /// <param name="groupId">Group identifier.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="ipV4Address">Ip address.</param>
        void JoinOrCreate(
            Guid groupId,
            Guid connectionId,
            IpV4Address ipV4Address);

        /// <summary>
        /// Get existing group.
        /// </summary>
        /// <param name="groupId">Group identifier.</param>
        /// <returns>
        /// Group.
        /// </returns>
        Group GetGroup(
            Guid groupId);
    }
}