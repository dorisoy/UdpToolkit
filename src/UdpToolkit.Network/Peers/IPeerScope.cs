using System.Collections.Generic;

namespace UdpToolkit.Network.Peers
{
    public interface IPeerScope
    {
        void AddPeer(Peer peer);
        IEnumerable<Peer> GetPeers();
        bool TryGetPeer(string peerId, out Peer peer);
        ushort GetScopeId();
    }
}