namespace UdpToolkit.Network.Packets
{
    using System;

    /// <summary>
    /// Protocol packet types.
    /// </summary>
    [Flags]
    internal enum PacketType : byte
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
        /// Ack packet.
        /// </summary>
        Ack = 8,

        /// <summary>
        /// User-defined packet.
        /// </summary>
        UserDefined = 16,
    }
}