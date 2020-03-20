using System.Net;
using UdpToolkit.Network.Peers;
using UdpToolkit.Network.Rudp;
using Xunit;

namespace UdpToolkit.Tests
{
    public class PeerTrackerTest
    {
        [Fact]
        public void PeerTracker_TryAddPeerToScope_ScopeInserted()
        {
            var peerTracker = new ServerPeerTracker();
            var scope = new PeerScope(scopeId: 7);
            var endPoint = new IPEndPoint(IPAddress.Any, 7777);
            var peer = new Peer(
                id: endPoint.ToString(),
                remotePeer: endPoint,
                reliableChannel: new ReliableChannel());
            
            var insertedScope = peerTracker.AddPeer(scope.GetScopeId(), peer);
            
            Assert.Equal(scope.GetScopeId(), insertedScope.GetScopeId());
        }
        
        [Fact]
        public void PeerTracker_TryAddPeerToScope_PeerAddedToScope()
        {
            var peerTracker = new ServerPeerTracker();
            var scope = new PeerScope(scopeId: 7);
            var endPoint = new IPEndPoint(IPAddress.Any, 7777);
            var peer = new Peer(
                id: endPoint.ToString(),
                remotePeer: endPoint,
                reliableChannel: new ReliableChannel());
            
            var insertedScope = peerTracker.AddPeer(scope.GetScopeId(), peer);
            var result = insertedScope.TryGetPeer(peer.Id, out var addedPeer);
            
            Assert.True(result);
            Assert.Equal(peer.Id, addedPeer.Id);
        }
        
        [Fact]
        public void PeerTracker_TryAddPeerToScope_PeerUpdatedInScope()
        {
            var peerTracker = new ServerPeerTracker();
            var scope = new PeerScope(scopeId: 7);

            var endPoint1 = new IPEndPoint(IPAddress.Any, 1111);
            var endPoint2 = new IPEndPoint(IPAddress.Any, 2222);
            
            var peer1 = new Peer(
                id: endPoint1.ToString(),
                remotePeer: endPoint1,
                reliableChannel: new ReliableChannel());
            
            var peer2 = new Peer(
                id: endPoint2.ToString(),
                remotePeer: endPoint2,
                reliableChannel: new ReliableChannel());
            
            peerTracker.AddPeer(scope.GetScopeId(), peer1);
            var updatedScope = peerTracker.AddPeer(scope.GetScopeId(), peer2);
            
            var result = updatedScope.TryGetPeer(peer2.Id, out var updatedPeer);
            
            Assert.True(result);
            Assert.Equal(peer2.Id, updatedPeer.Id);
        }
    }
}
