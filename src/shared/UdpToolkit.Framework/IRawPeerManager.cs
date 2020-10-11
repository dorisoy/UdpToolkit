namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public interface IRawPeerManager
    {
        void Remove(
            Peer peer);

        Peer GetPeer(
            Guid peerId);

        Peer AddOrUpdate(
            Guid peerId,
            List<IPEndPoint> ips,
            TimeSpan inactivityTimeout);
    }
}