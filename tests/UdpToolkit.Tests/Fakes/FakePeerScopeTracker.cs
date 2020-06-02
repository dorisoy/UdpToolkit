namespace UdpToolkit.Tests.Fakes
{
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Framework.Server.Peers;

    public class FakePeerScopeTracker : IPeerScopeTracker
    {
        public bool TryGetScope(ushort scopeId, out IPeerScope scope)
        {
            throw new System.NotImplementedException();
        }

        public IPeerScope GetOrAddScope(ushort scopeId, IPeerScope peerScope)
        {
            throw new System.NotImplementedException();
        }
    }
}