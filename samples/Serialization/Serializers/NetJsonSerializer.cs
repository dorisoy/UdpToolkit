namespace Serializers
{
    using System;
    using System.Buffers;
    using System.Text.Json;
    using UdpToolkit.Serialization;

    public sealed class NetJsonSerializer : ISerializer
    {
        public void Serialize<T>(IBufferWriter<byte> buffer, T item)
        {
            JsonSerializer.Serialize(new Utf8JsonWriter(buffer), item);
        }

        public T Deserialize<T>(ReadOnlySpan<byte> buffer, T item)
        {
            // JsonSerializer does not support deserialization to an existing object
            return JsonSerializer.Deserialize<T>(buffer);
        }
    }
}