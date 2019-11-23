namespace UdpToolkit.Core
{
    public interface IPeerTracker
    {
        bool TryGetScope(ushort scopeId, out IPeerScope scope);
        IPeerScope TryAddPeerToScope(ushort scopeId, Peer peerScope);
    }
}