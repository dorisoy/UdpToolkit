namespace UdpToolkit.Network.Peers
{
    public interface IPeerTracker
    {
        bool TryGetScope(ushort scopeId, out IPeerScope scope);

        IPeerScope AddPeer(ushort scopeId, Peer peer);

        bool TryGetPeer(ushort scopeId, string peerId, out Peer peer);
    }
}