namespace UdpToolkit.Framework.Hubs
{
    public sealed class HubContext
    {
        public HubContext(
            ushort scopeId,
            byte hubId,
            byte rpcId,
            string peerId)
        {
            ScopeId = scopeId;
            HubId = hubId;
            RpcId = rpcId;
            PeerId = peerId;
        }

        public ushort ScopeId { get; }

        public byte HubId { get; }

        public byte RpcId { get; }

        public string PeerId { get; }
    }
}
