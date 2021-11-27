namespace Serializers
{
    using System;
    using System.Buffers;
    using UdpToolkit.Serialization;

    public class MessagePackSerializer : ISerializer
    {
        public void Serialize<T>(IBufferWriter<byte> buffer, T item)
        {
            MessagePack.MessagePackSerializer.Serialize(buffer, item);
        }

        public T Deserialize<T>(ReadOnlySpan<byte> buffer, T item)
        {
            // MessagePack does not support deserialization to an existing object
            return MessagePack.MessagePackSerializer.Deserialize<T>(buffer.ToArray());
        }
    }
}