namespace UdpToolkit.Network.Protocol
{
    using System;

    public sealed class Heartbeat : ProtocolEvent<Heartbeat>
    {
        protected override byte[] SerializeInternal(Heartbeat heartbeat) => Array.Empty<byte>();

        protected override Heartbeat DeserializeInternal(byte[] bytes) => new Heartbeat();
    }
}