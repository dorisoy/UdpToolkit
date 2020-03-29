namespace UdpToolkit.Framework.Peers
{
    public interface IPeerScopeTracker
    {
        bool TryGetScope(ushort scopeId, out IPeerScope scope);

        IPeerScope GetOrAddScope(ushort scopeId, PeerScope peerScope);
    }
}