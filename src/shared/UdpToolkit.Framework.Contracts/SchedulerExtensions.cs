namespace UdpToolkit.Framework.Contracts
{
    using System;
    using System.Threading;

    /// <summary>
    /// Extensions for Scheduler.
    /// </summary>
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Schedule action.
        /// </summary>
        /// <param name="scheduler">Instance of scheduler.</param>
        /// <param name="action">Scheduled action.</param>
        /// <param name="delay">Delay value.</param>
        /// <typeparam name="TEvent">
        /// Type of user-defined event.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If the scheduler instance is null.
        /// </exception>
        public static void Schedule<TEvent>(
            this IScheduler scheduler,
            Action action,
            TimeSpan delay)
        {
#pragma warning disable SA1503
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
#pragma warning restore SA1503

            scheduler.Schedule(
                timerKey: new TimerKey(Guid.NewGuid(), typeof(TEvent)),
                delay: delay,
                frequency: TimeSpan.FromMilliseconds(Timeout.Infinite),
                ttl: delay + TimeSpan.FromSeconds(5),
                action: action);
        }

        /// <summary>
        /// Schedule action once per room.
        /// </summary>
        /// <param name="scheduler">Instance of scheduler.</param>
        /// <param name="roomId">Room identifier.</param>
        /// <param name="action">Scheduled action.</param>
        /// <param name="delay">Delay value.</param>
        /// <typeparam name="TEvent">
        /// Type of user-defined event.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If the scheduler instance is null.
        /// </exception>
        public static void ScheduleOnce<TEvent>(
            this IScheduler scheduler,
            Guid roomId,
            Action action,
            TimeSpan delay)
        {
#pragma warning disable SA1503
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
#pragma warning restore SA1503

            scheduler.Schedule(
                timerKey: new TimerKey(roomId, typeof(TEvent)),
                delay: delay,
                frequency: TimeSpan.FromMilliseconds(Timeout.Infinite),
                ttl: delay + TimeSpan.FromSeconds(5),
                action: action);
        }

        /// <summary>
        /// Schedule repeatable action per room.
        /// </summary>
        /// <param name="scheduler">Instance of scheduler.</param>
        /// <param name="roomId">Room identifier.</param>
        /// <param name="action">Scheduled action.</param>
        /// <param name="delay">Delay value.</param>
        /// <param name="frequency">Frequency of call.</param>
        /// <typeparam name="TEvent">
        /// Type of user-defined event.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If the scheduler instance is null.
        /// </exception>
        public static void ScheduleRepeatable<TEvent>(
            this IScheduler scheduler,
            Guid roomId,
            Action action,
            TimeSpan delay,
            TimeSpan frequency)
        {
#pragma warning disable SA1503
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
#pragma warning restore SA1503

            scheduler.Schedule(
                timerKey: new TimerKey(roomId, typeof(TEvent)),
                delay: delay,
                frequency: frequency,
                ttl: null, // roomTtl
                action: action);
        }
    }
}