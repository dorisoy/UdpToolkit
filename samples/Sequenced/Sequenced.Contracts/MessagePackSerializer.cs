namespace Sequenced.Contracts
{
    using System;
    using UdpToolkit.Serialization;

    public class MessagePackSerializer : ISerializer
    {
        public byte[] Serialize<T>(T item)
        {
            return MessagePack.MessagePackSerializer.Serialize(item);
        }

        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            return MessagePack.MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}