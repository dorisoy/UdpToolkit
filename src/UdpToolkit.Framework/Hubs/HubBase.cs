using UdpToolkit.Core;
using UdpToolkit.Network.Clients;
using UdpToolkit.Network.Packets;
using UdpToolkit.Network.Peers;
using UdpToolkit.Network.Protocol;
using UdpToolkit.Network.Queues;

namespace UdpToolkit.Framework.Hubs
{
    public abstract class HubBase
    {
        public HubContext HubContext { get; set; }

        public IAsyncQueue<OutputUdpPacket> EventProducer { get; set; }
        public IPeerTracker PeerTracker { get; set; }
        public ISerializer Serializer { get; set; }

        protected void Broadcast<TEvent>(TEvent @event, UdpMode mode)
        {
            if (!PeerTracker.TryGetScope(HubContext.ScopeId, out var scope))
                return;
            
            var peers = scope.GetPeers();
            var bytes = Serializer.Serialize(@event);
            var packet = new OutputUdpPacket(
                payload: bytes, 
                peers: peers, 
                mode: mode, 
                frameworkHeader: new FrameworkHeader(
                    hubId: HubContext.HubId,
                    rpcId: HubContext.RpcId,
                    scopeId: HubContext.ScopeId));

            EventProducer.Produce(@event: packet);
        }

        protected void Unicast<TEvent>(TEvent @event, UdpMode mode)
        {
            if (!PeerTracker.TryGetScope(HubContext.ScopeId, out var scope))
                return;
            
            if (!scope.TryGetPeer(HubContext.PeerId, out var peer))
                return;
            
            var bytes = Serializer.Serialize(@event);
            var packet = new OutputUdpPacket(
                payload: bytes, 
                peers: new []{ peer }, 
                mode: mode, 
                frameworkHeader: new FrameworkHeader(
                    hubId: HubContext.HubId,
                    rpcId: HubContext.RpcId,
                    scopeId: HubContext.ScopeId));

            EventProducer.Produce(@event: packet);
        }
    }
}
