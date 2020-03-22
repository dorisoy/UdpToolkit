namespace UdpToolkit.Core
{
    using System;

    public interface ISerializer
    {
        byte[] Serialize<T>(T @event);

        object Deserialize(Type type, ArraySegment<byte> bytes);

        T Deserialize<T>(ArraySegment<byte> bytes);
    }
}
