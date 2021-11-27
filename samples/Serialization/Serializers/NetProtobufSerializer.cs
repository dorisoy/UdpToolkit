namespace Serializers
{
    using System;
    using System.Buffers;
    using ProtoBuf;
    using UdpToolkit.Serialization;

    public class NetProtobufSerializer : ISerializer
    {
        public void Serialize<T>(IBufferWriter<byte> buffer, T item)
        {
            ProtoBuf.Serializer.Serialize(buffer, item);
        }

        public T Deserialize<T>(ReadOnlySpan<byte> buffer, T item)
        {
            return Serializer.Deserialize<T>(buffer, item);
        }
    }
}