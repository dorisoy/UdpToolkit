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
        /// <param name="timerKey">Compound key for timer.</param>
        /// <param name="delay">Period of time for delay.</param>
        /// <param name="frequency">Frequency of repetitions.</param>
        /// <param name="ttl">TTL for created timer.</param>
        /// <param name="action">Scheduled action.</param>
        void Schedule(
            TimerKey timerKey,
            TimeSpan delay,
            TimeSpan frequency,
            TimeSpan? ttl,
            Action action);
    }
}