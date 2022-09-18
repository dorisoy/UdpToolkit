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

        public void SerializeUnmanaged<T>(IBufferWriter<byte> buffer, T item)
            where T : unmanaged
        {
            throw new NotSupportedException();
        }

        public T Deserialize<T>(ReadOnlySpan<byte> buffer, T item)
        {
            return Serializer.Deserialize<T>(buffer, item);
        }

        public T DeserializeUnmanaged<T>(ReadOnlySpan<byte> buffer)
            where T : unmanaged
        {
            throw new NotSupportedException();
        }
    }
}