using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UdpToolkit.Network.Peers
{
    public sealed class PeerScope : IPeerScope
    {
        private readonly ushort _scopeId;
        private readonly ConcurrentDictionary<string, Peer> _scope;

        public PeerScope(ushort scopeId)
        {
            _scopeId = scopeId;
            _scope = new ConcurrentDictionary<string, Peer>();
        }

        public void AddPeer(Peer peer)
        {
            _scope.TryAdd(peer.Id, peer);
        }

        public IEnumerable<Peer> GetPeers()
        {
            return _scope.Select(x => x.Value);
        }

        public bool TryGetPeer(string peerId, out Peer peer)
        {
            return _scope.TryGetValue(peerId, out peer);
        }

        public ushort GetScopeId()
        {
            return _scopeId;
        }
    }
}
