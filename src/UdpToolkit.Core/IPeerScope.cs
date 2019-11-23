using System.Collections.Generic;

namespace UdpToolkit.Core
{
    public interface IPeerScope
    {
        void AddPeer(Peer peer);
        IEnumerable<Peer> GetPeers();
        bool TryGetPeer(string peerId, out Peer peer);
        ushort GetScopeId();
    }
}