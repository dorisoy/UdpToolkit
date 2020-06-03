namespace UdpToolkit.Framework.Server.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Network.Peers;

    public interface IPeerManager
    {
        void Add(Peer peer);

        void Remove(Peer peer);

        Peer Get(Guid peerId);

        Peer GetOrAdd(IPEndPoint ipEndPoint);

        IEnumerable<Peer> GetAll();
    }
}