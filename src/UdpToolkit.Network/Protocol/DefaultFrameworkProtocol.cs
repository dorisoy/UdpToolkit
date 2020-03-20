using System;

namespace UdpToolkit.Network.Protocol
{
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
                hubId: bytes[0],
                rpcId: bytes[1],
                scopeId: ReadScopeId(bytes));

            return true;
        }

        public byte[] Serialize(FrameworkHeader header)
        {
            var scopeIdBytes = BitConverter.GetBytes(header.ScopeId);

            return new []
            {
                header.HubId,
                header.RpcId,
                scopeIdBytes[0],
                scopeIdBytes[1]
            };
        }

        private ushort ReadScopeId(ArraySegment<byte> buffer)
        {
            return BitConverter.ToUInt16(new byte[] {buffer[2], buffer[3]}); //TODO span api
        }
    }
}
