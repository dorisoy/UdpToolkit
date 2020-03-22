namespace UdpToolkit.Network.Peers
{
    using System.Collections.Generic;

    public interface IPeerScope
    {
        void AddPeer(Peer peer);

        IEnumerable<Peer> GetPeers();

        bool TryGetPeer(string peerId, out Peer peer);

        ushort GetScopeId();
    }
}