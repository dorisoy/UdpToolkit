namespace UdpToolkit.Serialization.MsgPack
{
    using System;
    using MessagePack;

    public sealed class Serializer : ISerializer
    {
        public byte[] Serialize<T>(T item)
        {
            return MessagePackSerializer.Serialize(item);
        }

        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}