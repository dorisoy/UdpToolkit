namespace UdpToolkit.Core.ProtocolEvents
{
    using System;

    public sealed class Pong : ProtocolEvent<Pong>
    {
        protected override byte[] SerializeInternal(Pong pong) => Array.Empty<byte>();

        protected override Pong DeserializeInternal(byte[] bytes) => new Pong();
    }
}
