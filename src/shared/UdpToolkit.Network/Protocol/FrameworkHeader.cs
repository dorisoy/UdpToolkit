namespace UdpToolkit.Network.Protocol
{
    public readonly struct FrameworkHeader
    {
        public FrameworkHeader(
            byte hubId,
            byte rpcId,
            ushort roomId)
        {
            HubId = hubId;
            RpcId = rpcId;
            RoomId = roomId;
        }

        public byte HubId { get; }

        public byte RpcId { get; }

        public ushort RoomId { get; }
    }
}
