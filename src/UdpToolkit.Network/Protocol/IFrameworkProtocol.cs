namespace UdpToolkit.Network.Protocol
{
    using System;

    public interface IFrameworkProtocol
    {
        bool TryDeserialize(ArraySegment<byte> bytes, out FrameworkHeader header);

        byte[] Serialize(FrameworkHeader header);
    }
}
