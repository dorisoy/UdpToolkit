namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class PeerInfo
    {
        public PeerInfo(
            Guid peerId,
            List<IPEndPoint> peerIps)
        {
            PeerId = peerId;
            PeerIps = peerIps;
        }

        public Guid PeerId { get; }

        public List<IPEndPoint> PeerIps { get; }
    }
}