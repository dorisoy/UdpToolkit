namespace UdpToolkit.Network.Packets
{
    using System;

    [Flags]
    internal enum PacketType : byte
    {
        Connect = 1,
        Disconnect = 2,
        Heartbeat = 4,
        Ack = 8,
        UserDefined = 16,
    }
}