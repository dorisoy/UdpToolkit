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
                hubId: bytes[0],
                rpcId: bytes[1],
                roomId: ReadRoomId(bytes));

            return true;
        }

        public byte[] Serialize(FrameworkHeader header)
        {
            var roomIdBytes = BitConverter.GetBytes(header.RoomId);

            return new[]
            {
                header.HubId,
                header.RpcId,
                roomIdBytes[0],
                roomIdBytes[1],
            };
        }

        private ushort ReadRoomId(ArraySegment<byte> buffer)
        {
            return BitConverter.ToUInt16(new byte[] { buffer[2], buffer[3] }); // TODO span api
        }
    }
}
