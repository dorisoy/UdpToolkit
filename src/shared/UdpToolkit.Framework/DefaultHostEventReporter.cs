namespace UdpToolkit.Framework
{
    using UdpToolkit.Framework.Contracts.Events;

    /// <inheritdoc />
    public sealed class DefaultHostEventReporter : IHostEventReporter
    {
        /// <inheritdoc />
        public void Handle(
            in ExceptionThrown @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in HostStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ScanExpiredTimersStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ExpiredTimerRemoved @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ScanExpiredGroupsStarted @event)
        {
            // nothing todd by default
        }
    }
}