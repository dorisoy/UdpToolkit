using System;

namespace UdpToolkit.Core
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T @event);

        object Deserialize(Type type, ArraySegment<byte> bytes);

        T Deserialize<T>(ArraySegment<byte> bytes);
    }
}
