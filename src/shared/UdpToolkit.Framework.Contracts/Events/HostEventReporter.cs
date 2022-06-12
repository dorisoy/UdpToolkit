namespace UdpToolkit.Framework.Contracts.Events
{
    /// <inheritdoc />
    public abstract class HostEventReporter : IHostEventReporter
    {
        /// <inheritdoc />
        public virtual void Handle(
            in ExceptionThrown @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in HostStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ScanExpiredTimersStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ExpiredTimerRemoved @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ScanExpiredGroupsStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in QueueItemConsumed @event)
        {
            // nothing todd by default
        }
    }
}