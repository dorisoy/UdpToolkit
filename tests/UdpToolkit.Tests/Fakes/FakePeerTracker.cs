using UdpToolkit.Network;
using UdpToolkit.Network.Peers;

namespace UdpToolkit.Tests.Fakes
{
    public class FakePeerTracker : IPeerTracker
    {
        public bool TryGetScope(ushort scopeId, out IPeerScope scope)
        {
            throw new System.NotImplementedException();
        }

        public IPeerScope AddPeer(ushort scopeId, Peer peerScope)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetPeer(ushort scopeId, string peerId, out Peer peer)
        {
            throw new System.NotImplementedException();
        }
    }
}