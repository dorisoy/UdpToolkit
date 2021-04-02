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

        public void Foo()
        {
            var result = 1 + 3;
            Console.WriteLine(result);
        }
    }
}

// Func<byte[]> Serialize() { ... };
// |protocol (connect|disconnect) memory stream bin formatter (allocation free, !fast, !3d party lib) |framework message pack (allocation free, fast)|