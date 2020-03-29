namespace UdpToolkit.Tests.Fakes
{
    using UdpToolkit.Framework.Peers;
    using UdpToolkit.Network.Peers;

    public class FakePeerScopeTracker : IPeerScopeTracker
    {
        public bool TryGetScope(ushort scopeId, out IPeerScope scope)
        {
            throw new System.NotImplementedException();
        }

        public IPeerScope GetOrAddScope(ushort scopeId, PeerScope peerScope)
        {
            throw new System.NotImplementedException();
        }
    }
}