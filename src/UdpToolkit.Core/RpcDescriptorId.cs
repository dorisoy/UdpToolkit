namespace UdpToolkit.Core
{
    public readonly struct RpcDescriptorId
    {
        public RpcDescriptorId(
            byte hubId,
            byte rpcId)
        {
            HubId = hubId;
            RpcId = rpcId;
        }

        public byte HubId { get; }

        public byte RpcId { get; }

        public bool Equals(RpcDescriptorId other)
        {
            return HubId == other.HubId && RpcId == other.RpcId;
        }

        public override bool Equals(object obj)
        {
            return obj is RpcDescriptorId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (HubId.GetHashCode() * 397) ^ RpcId.GetHashCode();
            }
        }
    }
}
