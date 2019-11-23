namespace UdpToolkit.Core
{
    public abstract class HubBase
    {
        public HubContext HubContext { get; set; }
        public IUdpSenderProxy UdpSenderProxy { get; set; }
        public IPeerTracker PeerTracker { get; set; }
        public ISerializer Serializer { get; set; }

        protected void Broadcast<TEvent>(TEvent @event, UdpMode mode)
        {
            if (!PeerTracker.TryGetScope(HubContext.ScopeId, out var scope))
                return;
            
            var peers = scope.GetPeers();
            var bytes = Serializer.Serialize(@event);
            var packet = new OutputUdpPacket(bytes, peers, mode);

            UdpSenderProxy.Publish(packet);
        }

        protected void Unicast<TEvent>(TEvent @event, UdpMode mode)
        {
            if (!PeerTracker.TryGetScope(HubContext.ScopeId, out var scope))
                return;
            
            if (!scope.TryGetPeer(HubContext.PeerId, out var peer))
                return;
            
            var bytes = Serializer.Serialize(@event);
            var packet = new OutputUdpPacket(bytes, new []{ peer }, mode);

            UdpSenderProxy.Publish(packet);
        }
    }
}
