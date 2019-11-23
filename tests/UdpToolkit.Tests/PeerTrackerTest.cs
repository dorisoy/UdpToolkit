using System.Net;
using UdpToolkit.Core;
using UdpToolkit.Network;
using Xunit;

namespace UdpToolkit.Tests
{
    public class PeerTrackerTest
    {
        [Fact]
        public void PeerTracker_TryAddPeerToScope_ScopeInserted()
        {
            var peerTracker = new PeerTracker();
            var scope = new PeerScope(scopeId: 7);
            var peer = new Peer(new IPEndPoint(IPAddress.Any, 7777));
            
            var insertedScope = peerTracker.TryAddPeerToScope(scope.GetScopeId(), peer);
            
            Assert.Equal(scope.GetScopeId(), insertedScope.GetScopeId());
        }
        
        [Fact]
        public void PeerTracker_TryAddPeerToScope_PeerAddedToScope()
        {
            var peerTracker = new PeerTracker();
            var scope = new PeerScope(scopeId: 7);
            var peer = new Peer(new IPEndPoint(IPAddress.Any, 7777));
            
            var insertedScope = peerTracker.TryAddPeerToScope(scope.GetScopeId(), peer);
            var result = insertedScope.TryGetPeer(peer.Id, out var addedPeer);
            
            Assert.True(result);
            Assert.Equal(peer.Id, addedPeer.Id);
        }
        
        [Fact]
        public void PeerTracker_TryAddPeerToScope_PeerUpdatedInScope()
        {
            var peerTracker = new PeerTracker();
            var scope = new PeerScope(scopeId: 7);

            var peer1 = new Peer(new IPEndPoint(IPAddress.Any, 1111));
            var peer2 = new Peer(new IPEndPoint(IPAddress.Any, 2222));
            
            peerTracker.TryAddPeerToScope(scope.GetScopeId(), peer1);
            var updatedScope = peerTracker.TryAddPeerToScope(scope.GetScopeId(), peer2);
            
            var result = updatedScope.TryGetPeer(peer2.Id, out var updatedPeer);
            
            Assert.True(result);
            Assert.Equal(peer2.Id, updatedPeer.Id);
        }
    }
}