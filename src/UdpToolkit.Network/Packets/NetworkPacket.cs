namespace UdpToolkit.Network.Packets
{
    using System.Collections.Generic;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Protocol;

    public readonly struct NetworkPacket
    {
        public NetworkPacket(
            FrameworkHeader frameworkHeader,
            UdpMode udpMode,
            byte[] payload,
            IEnumerable<Peer> peers)
        {
            Payload = payload;
            Peers = peers;
            FrameworkHeader = frameworkHeader;
            UdpMode = udpMode;
        }

        public FrameworkHeader FrameworkHeader { get; }

        public UdpMode UdpMode { get; }

        public byte[] Payload { get; }

        public IEnumerable<Peer> Peers { get; }
    }
}