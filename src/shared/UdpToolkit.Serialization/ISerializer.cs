namespace UdpToolkit.Serialization
{
    using System;

    public interface ISerializer
    {
        byte[] Serialize<T>(T @event);

        byte[] SerializeContractLess<T>(T @event);

        T DeserializeContractLess<T>(ArraySegment<byte> bytes);

        T Deserialize<T>(ArraySegment<byte> bytes);
    }
}
