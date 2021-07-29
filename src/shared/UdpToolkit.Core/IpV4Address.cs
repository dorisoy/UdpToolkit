namespace UdpToolkit.Core
{
    public readonly struct IpV4Address
    {
        public IpV4Address(
            string host,
            ushort port)
        {
            Host = host;
            Port = port;
        }

        public string Host { get; }

        public ushort Port { get; }
    }
}