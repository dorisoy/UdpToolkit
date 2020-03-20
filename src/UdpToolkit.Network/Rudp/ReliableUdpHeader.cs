namespace UdpToolkit.Network.Rudp
{
    public readonly struct ReliableUdpHeader
    {
        public ReliableUdpHeader(
            uint localNumber,
            uint ack,
            uint acks)
        {
            LocalNumber = localNumber;
            Ack = ack;
            Acks = acks;
        }

        public uint LocalNumber { get; }
        
        public uint Ack { get; }
        
        public uint Acks { get; }
    }
}