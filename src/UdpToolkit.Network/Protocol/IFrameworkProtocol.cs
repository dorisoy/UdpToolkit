using System;

namespace UdpToolkit.Network.Protocol
{
    public interface IFrameworkProtocol
    {
        bool TryDeserialize(ArraySegment<byte> bytes, out FrameworkHeader header);

        byte[] Serialize(FrameworkHeader header);
    }
}
