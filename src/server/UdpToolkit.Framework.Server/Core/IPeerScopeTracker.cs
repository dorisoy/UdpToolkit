namespace UdpToolkit.Framework.Server.Core
{
    public interface IPeerScopeTracker
    {
        bool TryGetScope(ushort scopeId, out IPeerScope scope);

        IPeerScope GetOrAddScope(ushort scopeId, IPeerScope peerScope);
    }
}