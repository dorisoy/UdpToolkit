namespace UdpToolkit.Serialization.MsgPack
{
    using System;
    using MessagePack;

    public sealed class Serializer : ISerializer
    {
        public byte[] Serialize<T>(T @event)
        {
            return MessagePackSerializer.Serialize(@event);
        }

        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}