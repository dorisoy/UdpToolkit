namespace UdpToolkit.Network.Contracts.Events
{
    using UdpToolkit.Network.Contracts.Events.UdpClient;

    /// <summary>
    /// Abstraction for reporting network events.
    /// </summary>
    public interface INetworkEventReporter
    {
        /// <summary>
        /// MtuSizeExceeded handler.
        /// </summary>
        /// <param name="event">MtuSizeExceeded event.</param>
        void Handle(
            in MtuSizeExceeded @event);

        /// <summary>
        /// InvalidHeaderReceived handler.
        /// </summary>
        /// <param name="event">InvalidHeaderReceived event.</param>
        void Handle(
            in InvalidHeaderReceived @event);

        /// <summary>
        /// ConnectionRejected handler.
        /// </summary>
        /// <param name="event">ConnectionRejected event.</param>
        void Handle(
            in ConnectionRejected @event);

        /// <summary>
        /// ConnectionAccepted handler.
        /// </summary>
        /// <param name="event">ConnectionAccepted event.</param>
        void Handle(
            in ConnectionAccepted @event);

        /// <summary>
        /// ChannelNotFound handler.
        /// </summary>
        /// <param name="event">ChannelNotFound event.</param>
        void Handle(
            in ChannelNotFound @event);

        /// <summary>
        /// ExceptionThrown handler.
        /// </summary>
        /// <param name="event">ExceptionThrown event.</param>
        void Handle(
            in NetworkExceptionThrown @event);

        /// <summary>
        /// ConnectionNotFound handler.
        /// </summary>
        /// <param name="event">ConnectionNotFound event.</param>
        void Handle(
            in ConnectionNotFound @event);

        /// <summary>
        /// UdpClientStarted handler.
        /// </summary>
        /// <param name="event">UdpClientStarted event.</param>
        void Handle(
            in UdpClientStarted @event);

        /// <summary>
        /// ScanInactiveConnectionsStarted handler.
        /// </summary>
        /// <param name="event">ScanInactiveConnectionsStarted event.</param>
        void Handle(
            in ScanInactiveConnectionsStarted @event);

        /// <summary>
        /// ConnectionRemovedByTimeout handler.
        /// </summary>
        /// <param name="event">ConnectionRemovedByTimeout event.</param>
        void Handle(
            in ConnectionRemovedByTimeout @event);

        /// <summary>
        /// HeartbeatReceived handler.
        /// </summary>
        /// <param name="event">HeartbeatReceived event.</param>
        void Handle(
            in PingReceived @event);
    }
}