namespace UdpToolkit.Network.Contracts.Events
{
    using UdpToolkit.Network.Contracts.Events.UdpClient;

    /// <inheritdoc />
    public class DefaultNetworkEventReporter : INetworkEventReporter
    {
        /// <inheritdoc />
        public void Handle(
            in MtuSizeExceeded @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in InvalidHeaderReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ConnectionRejected @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ConnectionAccepted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ChannelNotFound @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in NetworkExceptionThrown @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ConnectionNotFound @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in UdpClientStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ScanInactiveConnectionsStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(
            in ConnectionRemovedByTimeout @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public void Handle(in PingReceived @event)
        {
            // nothing todd by default
        }
    }
}