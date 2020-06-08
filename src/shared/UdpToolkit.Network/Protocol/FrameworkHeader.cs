namespace UdpToolkit.Network.Protocol
{
    public readonly struct FrameworkHeader
    {
        public FrameworkHeader(
            byte hubId,
            byte rpcId)
        {
            HubId = hubId;
            RpcId = rpcId;
        }

        public byte HubId { get; }

        public byte RpcId { get; }
    }
}
