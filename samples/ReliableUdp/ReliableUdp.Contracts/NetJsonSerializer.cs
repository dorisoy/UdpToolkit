namespace ReliableUdp.Contracts
{
    using System;
    using System.Text.Json;
    using UdpToolkit.Serialization;

    /// <summary>
    /// Default version of serializer.
    /// </summary>
    public sealed class NetJsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item);
        }

        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes);
        }
    }
}