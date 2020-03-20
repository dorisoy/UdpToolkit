namespace UdpToolkit.Network.Protocol
{
    public readonly struct FrameworkHeader
    {
        public FrameworkHeader(
            byte hubId,
            byte rpcId,
            ushort scopeId)
        {
            HubId = hubId;
            RpcId = rpcId;
            ScopeId = scopeId;
        }

        public byte HubId { get; }
        
        public byte RpcId { get; }
        
        public ushort ScopeId { get; }
    }
}
