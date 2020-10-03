namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Core;

    public interface IPeerManager
    {
        IPeer Create(Guid peerId, List<IPEndPoint> peerIps);

        void Remove(Guid peerId);

        bool Exist(Guid peerId);

        bool TryGetPeer(Guid peerId, out IPeer peer);

        IEnumerable<IPeer> GetAll();
    }
}