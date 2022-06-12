namespace UdpToolkit.Framework.Contracts.Events
{
    /// <summary>
    /// Abstraction for report host events.
    /// </summary>
    public interface IHostEventReporter
    {
        /// <summary>
        /// ExceptionThrown handler.
        /// </summary>
        /// <param name="event">ExceptionThrown event.</param>
        void Handle(
            in ExceptionThrown @event);

        /// <summary>
        /// HostStarted handler.
        /// </summary>
        /// <param name="event">HostStarted event.</param>
        void Handle(
            in HostStarted @event);

        /// <summary>
        /// ScanExpiredTimersStarted handler.
        /// </summary>
        /// <param name="event">ScanExpiredTimersStarted event.</param>
        void Handle(
            in ScanExpiredTimersStarted @event);

        /// <summary>
        /// ExpiredTimerRemoved handler.
        /// </summary>
        /// <param name="event">ExpiredTimerRemoved event.</param>
        void Handle(
            in ExpiredTimerRemoved @event);

        /// <summary>
        /// ScanExpiredGroupsStarted handler.
        /// </summary>
        /// <param name="event">ScanExpiredGroupsStarted event.</param>
        void Handle(
            in ScanExpiredGroupsStarted @event);

        /// <summary>
        /// QueueItemConsumed handler.
        /// </summary>
        /// <param name="event">QueueItemConsumed event.</param>
        void Handle(
            in QueueItemConsumed @event);
    }
}