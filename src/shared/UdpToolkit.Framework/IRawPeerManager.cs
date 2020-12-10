namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

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

        Task Apply(
            Func<Peer, bool> condition,
            Func<Peer, Task> action);
    }
}