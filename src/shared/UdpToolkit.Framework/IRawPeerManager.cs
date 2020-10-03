namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public interface IRawPeerManager
    {
        bool TryGetPeer(Guid peerId, out Peer peer);

        Peer Create(Guid peerId, List<IPEndPoint> peerIps);
    }
}