// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;

    /// <summary>
    /// Scheduler, abstraction for sending delayed packets.
    /// </summary>
    public interface IScheduler : IDisposable
    {
        /// <summary>
        /// Schedules packets for sending.
        /// </summary>
        /// <param name="event">Instance of user-defined event for sending.</param>
        /// <param name="caller">Sending initiator.</param>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="roomId">Room identifier.</param>
        /// <param name="eventName">Name of event.</param>
        /// <param name="dueTime">Period of time for delay.</param>
        /// <param name="broadcastMode">Broadcast mode.</param>
        /// <typeparam name="TEvent">
        /// Type of user-defined event.
        /// </typeparam>
        void Schedule<TEvent>(
            TEvent @event,
            Guid caller,
            byte channelId,
            int roomId,
            string eventName,
            TimeSpan dueTime,
            BroadcastMode broadcastMode);
    }
}