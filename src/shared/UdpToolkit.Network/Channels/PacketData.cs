namespace UdpToolkit.Network.Channels
{
    using System;
    using UdpToolkit.Network.Packets;

    public readonly struct PacketData
    {
        public PacketData(
            NetworkPacket networkPacket,
            bool acked)
        {
            NetworkPacket = networkPacket;
            Acked = acked;
        }

        public NetworkPacket NetworkPacket { get; }

        public bool Acked { get; }
    }
}