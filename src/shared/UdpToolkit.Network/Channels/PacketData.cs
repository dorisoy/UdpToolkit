namespace UdpToolkit.Network.Channels
{
    public readonly struct PacketData
    {
        public PacketData(
            ushort id,
            uint acks,
            bool acked)
        {
            Acked = acked;
            Id = id;
            Acks = acks;
        }

        public ushort Id { get; }

        public uint Acks { get; }

        public bool Acked { get; }
    }
}