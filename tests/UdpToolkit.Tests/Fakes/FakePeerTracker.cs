using UdpToolkit.Core;

namespace UdpToolkit.Tests.Fakes
{
    public class FakePeerTracker : IPeerTracker
    {
        public bool TryGetScope(ushort scopeId, out IPeerScope scope)
        {
            throw new System.NotImplementedException();
        }

        public IPeerScope TryAddPeerToScope(ushort scopeId, Peer peerScope)
        {
            throw new System.NotImplementedException();
        }
    }
}