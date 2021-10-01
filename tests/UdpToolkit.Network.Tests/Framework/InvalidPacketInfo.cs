namespace UdpToolkit.Network.Tests.Framework
{
    using UdpToolkit.Network.Contracts.Sockets;

    internal class InvalidPacketInfo
    {
        internal InvalidPacketInfo(
            IpV4Address ip,
            byte[] payload)
        {
            Ip = ip;
            Payload = payload;
        }

        public IpV4Address Ip { get; }

        public byte[] Payload { get; }
    }
}