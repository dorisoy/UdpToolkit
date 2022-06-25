namespace UdpToolkit.Network.Contracts.Events
{
    using UdpToolkit.Network.Contracts.Events.UdpClient;

    /// <inheritdoc />
    public abstract class NetworkEventReporter : INetworkEventReporter
    {
        /// <inheritdoc />
        public virtual void Handle(
            in MtuSizeExceeded @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in InvalidHeaderReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ConnectionRejected @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ConnectionAccepted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ChannelNotFound @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ExceptionThrown @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ConnectionNotFound @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ReceivingStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ScanInactiveConnectionsStarted @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ConnectionRemovedByTimeout @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(in PingReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in PingAckReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in DisconnectReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in DisconnectAckReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ConnectReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ConnectAckReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in UserDefinedReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in UserDefinedAckReceived @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in PendingPacketResent @event)
        {
            // nothing todd by default
        }

        /// <inheritdoc />
        public virtual void Handle(
            in ExpiredPacketRemoved @event)
        {
            // nothing todd by default
        }
    }
}