namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Core;

    public interface IPeerManager
    {
        IPeer AddOrUpdate(
            Guid peerId,
            List<IPEndPoint> ips,
            TimeSpan inactivityTimeout);

        bool TryGetPeer(
            Guid peerId,
            out IPeer peer);
    }
}