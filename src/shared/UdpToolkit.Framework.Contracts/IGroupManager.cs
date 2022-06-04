namespace UdpToolkit.Framework.Contracts
{
    using System;

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
        void JoinOrCreate(
            Guid groupId,
            Guid connectionId);

        /// <summary>
        /// Leave group.
        /// </summary>
        /// <param name="groupId">Group identifier.</param>
        /// <param name="connectionId">Connection identifier.</param>
        void Leave(
            Guid groupId,
            Guid connectionId);

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