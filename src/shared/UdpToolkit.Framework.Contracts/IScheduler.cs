namespace UdpToolkit.Framework.Contracts
{
    using System;

    /// <summary>
    /// Abstraction for executing delayed actions.
    /// </summary>
    public interface IScheduler : IDisposable
    {
        /// <summary>
        /// Schedules action with a specified delay.
        /// </summary>
        /// <param name="inEvent">Instance of inEvent.</param>
        /// <param name="caller">Sending initiator.</param>
        /// <param name="timerKey">Compound key for timer.</param>
        /// <param name="dueTime">Period of time for delay.</param>
        /// <param name="action">Scheduled action.</param>
        /// <typeparam name="TInEvent">Type of InEvent.</typeparam>
        void Schedule<TInEvent>(
            TInEvent inEvent,
            Guid caller,
            TimerKey timerKey,
            TimeSpan dueTime,
            Action<Guid, TInEvent, IRoomManager, IBroadcaster> action);
    }
}