namespace UdpToolkit.Network.Contracts.Sockets
{
    public readonly struct IpV4Address
    {
        public IpV4Address(
            int address,
            ushort port)
        {
            Address = address;
            Port = port;
        }

        public int Address { get; }

        public ushort Port { get; }

        public override string ToString()
        {
            return this.ToIpEndPoint().ToString();
        }
    }
}