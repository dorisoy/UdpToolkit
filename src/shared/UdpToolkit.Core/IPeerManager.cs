namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Core;

    public interface IPeerManager
    {
        IPeer Create(Guid peerId, IPEndPoint peerIp);

        IPeer Create(Guid peerId, List<IPEndPoint> peerIps);

        bool TryRemove(Guid peerId, out IPeer peer);

        IPeer AddOrUpdate(Guid peerId, List<IPEndPoint> ips);

        bool Exist(Guid peerId);

        bool TryGetPeer(Guid peerId, out IPeer peer);

        IEnumerable<IPeer> GetAll();
    }
}