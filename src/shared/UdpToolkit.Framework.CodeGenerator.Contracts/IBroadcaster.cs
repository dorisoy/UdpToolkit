// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;

    /// <summary>
    /// Abstraction for broadcasting out packets.
    /// </summary>
    public interface IBroadcaster : IDisposable
    {
        /// <summary>
        /// Broadcast.
        /// </summary>
        /// <param name="caller">Caller connection identifier.</param>
        /// <param name="groupId">Group identifier.</param>
        /// <param name="event">User-defined event.</param>
        /// <param name="channelId">UDP Channel identifier.</param>
        /// <param name="broadcastMode">Broadcast mode.</param>
        /// <typeparam name="TEvent">Type of user-defined event.</typeparam>
        void Broadcast<TEvent>(
            Guid caller,
            Guid groupId,
            TEvent @event,
            byte channelId,
            BroadcastMode broadcastMode)
                where TEvent : class, IDisposable;
    }
}