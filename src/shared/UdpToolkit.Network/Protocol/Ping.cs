namespace UdpToolkit.Network.Protocol
{
    using System;

    public sealed class Ping : ProtocolEvent<Ping>
    {
        protected override byte[] SerializeInternal(Ping ping) => Array.Empty<byte>();

        protected override Ping DeserializeInternal(byte[] bytes) => new Ping();
    }
}