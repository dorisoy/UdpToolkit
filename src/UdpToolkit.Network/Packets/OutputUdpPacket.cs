using System.Collections.Generic;
using UdpToolkit.Network.Clients;
using UdpToolkit.Network.Peers;
using UdpToolkit.Network.Protocol;

namespace UdpToolkit.Network.Packets
{
    public readonly struct OutputUdpPacket
    {
        public OutputUdpPacket(
            byte[] payload, 
            IEnumerable<Peer> peers,
            UdpMode mode, 
            FrameworkHeader frameworkHeader)
        {
            Peers = peers;
            Mode = mode;
            Payload = payload;
            FrameworkHeader = frameworkHeader;
        }

        public FrameworkHeader FrameworkHeader { get; }

        public IEnumerable<Peer> Peers { get; }
        public byte[] Payload { get; }

        public UdpMode Mode { get; }
    }
}