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
            in ExceptionThrown @event);

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
            in ReceivingStarted @event);

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
        /// PingReceived handler.
        /// </summary>
        /// <param name="event">PingReceived event.</param>
        void Handle(
            in PingReceived @event);

        /// <summary>
        /// PingAckReceived handler.
        /// </summary>
        /// <param name="event">PingAckReceived event.</param>
        void Handle(
            in PingAckReceived @event);

        /// <summary>
        /// DisconnectReceived handler.
        /// </summary>
        /// <param name="event">DisconnectReceived event.</param>
        void Handle(
            in DisconnectReceived @event);

        /// <summary>
        /// DisconnectAckReceived handler.
        /// </summary>
        /// <param name="event">DisconnectAckReceived event.</param>
        void Handle(
            in DisconnectAckReceived @event);

        /// <summary>
        /// ConnectReceived handler.
        /// </summary>
        /// <param name="event">ConnectReceived event.</param>
        void Handle(
            in ConnectReceived @event);

        /// <summary>
        /// ConnectAckReceived handler.
        /// </summary>
        /// <param name="event">ConnectAckReceived event.</param>
        void Handle(
            in ConnectAckReceived @event);

        /// <summary>
        /// UserDefinedReceived handler.
        /// </summary>
        /// <param name="event">UserDefinedReceived event.</param>
        void Handle(
            in UserDefinedReceived @event);

        /// <summary>
        /// UserDefinedAckReceived handler.
        /// </summary>
        /// <param name="event">UserDefinedAckReceived event.</param>
        void Handle(
            in UserDefinedAckReceived @event);

        /// <summary>
        /// UserDefinedAckReceived handler.
        /// </summary>
        /// <param name="event">UserDefinedAckReceived event.</param>
        void Handle(
            in PendingPacketResent @event);

        /// <summary>
        /// ExpiredPacketRemoved handler.
        /// </summary>
        /// <param name="event">ExpiredPacketRemoved event.</param>
        void Handle(
            in ExpiredPacketRemoved @event);
    }
}