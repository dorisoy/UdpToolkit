namespace UdpToolkit.Network.Peers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    public interface IRawPeerManager
    {
        void Remove(
            IRawPeer peer);

        bool TryGetPeer(
            Guid peerId,
            out IRawPeer rawPeer);

        IRawPeer AddOrUpdate(
            Guid peerId,
            List<IPEndPoint> ips,
            TimeSpan inactivityTimeout);

        Task Apply(
            Func<IRawPeer, bool> condition,
            Func<IRawPeer, Task> action);
    }
}