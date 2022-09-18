namespace Serializers
{
    using System;
    using System.Buffers;
    using UdpToolkit.Serialization;

    public sealed class UnsafeSerializer : ISerializer
    {
        public void Serialize<T>(IBufferWriter<byte> buffer, T item)
        {
            throw new NotSupportedException();
        }

        public void SerializeUnmanaged<T>(IBufferWriter<byte> buffer, T item)
            where T : unmanaged
        {
            UdpToolkit.Network.Serialization.UnsafeSerialization.Serialize(buffer, item);
        }

        public T Deserialize<T>(ReadOnlySpan<byte> buffer, T item)
        {
            throw new NotSupportedException();
        }

        public T DeserializeUnmanaged<T>(ReadOnlySpan<byte> buffer)
            where T : unmanaged
        {
            return UdpToolkit.Network.Serialization.UnsafeSerialization.Deserialize<T>(buffer);
        }
    }
}
