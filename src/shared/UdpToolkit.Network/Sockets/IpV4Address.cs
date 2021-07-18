namespace UdpToolkit.Network.Sockets
{
    public struct IpV4Address
    {
#pragma warning disable S1104
        public int Address;
        public ushort Port;
#pragma warning restore S1104

        public override string ToString()
        {
            return this.ToIpEndPoint().ToString();
        }
    }
}