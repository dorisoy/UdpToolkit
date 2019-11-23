using System.Collections.Generic;
using UdpToolkit.Core;

namespace UdpToolkit.Core
{
    public readonly struct OutputUdpPacket
    {
        public OutputUdpPacket(
            byte[] bytes, 
            IEnumerable<Peer> peers,
            UdpMode mode)
        {
            Peers = peers;
            Mode = mode;
            Bytes = bytes;
        }

        public IEnumerable<Peer> Peers { get; }
        public byte[] Bytes { get; }

        public UdpMode Mode { get; }
    }
}