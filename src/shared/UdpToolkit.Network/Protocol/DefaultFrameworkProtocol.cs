namespace UdpToolkit.Network.Protocol
{
    using System;

    public sealed class DefaultFrameworkProtocol : IFrameworkProtocol
    {
        public bool TryDeserialize(ArraySegment<byte> bytes, out FrameworkHeader header)
        {
            header = default;
            if (bytes == null || bytes.Count < Consts.FrameworkHeaderLength)
            {
                return false;
            }

            header = new FrameworkHeader(
                hubId: bytes.Array[0],
                rpcId: bytes.Array[1]);

            return true;
        }

        public byte[] Serialize(FrameworkHeader header)
        {
            return new[]
            {
                header.HubId,
                header.RpcId,
            };
        }
    }
}
