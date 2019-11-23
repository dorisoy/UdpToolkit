using System.Collections.Concurrent;
using System.Collections.Generic;
using UdpToolkit.Core;

namespace UdpToolkit.Network
{
    public class PeerTracker : IPeerTracker
    {
        private readonly ConcurrentDictionary<ushort, IPeerScope> _scopes = new ConcurrentDictionary<ushort, IPeerScope>();

        public bool TryGetScope(ushort scopeId, out IPeerScope scope)
        {
            return _scopes.TryGetValue(scopeId, out scope);
        }

        public IPeerScope TryAddPeerToScope(ushort scopeId, Peer peer)
        {
            return _scopes.AddOrUpdate(
                key: scopeId, 
                addValueFactory: i =>
                {
                    var scope = new PeerScope(i);
                    scope.AddPeer(peer);
                    return scope;
                },
                updateValueFactory:(i, scope) =>
            {
                scope.AddPeer(peer);
                return scope;
            });
        }
    }
}