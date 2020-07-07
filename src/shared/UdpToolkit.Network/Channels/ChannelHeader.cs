namespace UdpToolkit.Network.Channels
{
    public readonly struct ChannelHeader
    {
        public ChannelHeader(
            ushort id,
            uint acks)
        {
            Id = id;
            Acks = acks;
        }

        public ushort Id { get; }

        public uint Acks { get; }
    }
}