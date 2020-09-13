namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Core;

    public interface IPeerManager
    {
        void Create(Guid peerId, List<IPEndPoint> peerIps);

        void Remove(Guid peerId);

        bool Exist(Guid peerId);

        Peer Get(Guid peerId);

        IEnumerable<Peer> GetAll();
    }
}