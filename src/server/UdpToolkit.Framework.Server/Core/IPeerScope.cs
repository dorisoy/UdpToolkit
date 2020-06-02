namespace UdpToolkit.Framework.Server.Core
{
    using System.Collections.Generic;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Utils;

    public interface IPeerScope : ICacheEntry
    {
        ushort ScopeId { get; }

        void AddPeer(Peer peer);

        IEnumerable<Peer> GetPeers();

        bool TryGetPeer(string peerId, out Peer peer);
    }
}