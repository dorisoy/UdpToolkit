namespace UdpToolkit.Network.Contracts.Protocol
{
    using System;

    /// <summary>
    /// Protocol packet types.
    /// </summary>
    [Flags]
    public enum PacketType : byte
    {
        /// <summary>
        /// Connection packet.
        /// </summary>
        Connect = 1,

        /// <summary>
        /// Disconnection packet.
        /// </summary>
        Disconnect = 2,

        /// <summary>
        /// Heartbeat packet.
        /// </summary>
        Heartbeat = 4,

        /// <summary>
        /// User-defined packet.
        /// </summary>
        UserDefined = 8,

        /// <summary>
        /// Ack packet.
        /// </summary>
        Ack = 16,
    }
}