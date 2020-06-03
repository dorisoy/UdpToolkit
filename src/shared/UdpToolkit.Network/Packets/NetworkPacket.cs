namespace UdpToolkit.Network.Packets
{
    using System.Net;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Rudp;

    public readonly struct NetworkPacket
    {
        public NetworkPacket(
            FrameworkHeader frameworkHeader,
            UdpMode udpMode,
            byte[] payload,
            IPEndPoint ipEndPoint)
        {
            Payload = payload;
            IpEndPoint = ipEndPoint;
            FrameworkHeader = frameworkHeader;
            UdpMode = udpMode;
        }

        public FrameworkHeader FrameworkHeader { get; }

        public UdpMode UdpMode { get; }

        public byte[] Payload { get; }

        public IPEndPoint IpEndPoint { get; }
    }
}