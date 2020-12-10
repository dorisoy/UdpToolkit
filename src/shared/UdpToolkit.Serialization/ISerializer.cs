namespace UdpToolkit.Serialization
{
    using System;

    public interface ISerializer
    {
        byte[] Serialize<T>(T @event);

        T Deserialize<T>(ArraySegment<byte> bytes);
    }
}
