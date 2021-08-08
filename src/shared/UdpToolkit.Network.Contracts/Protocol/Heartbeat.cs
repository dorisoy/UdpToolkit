namespace UdpToolkit.Network.Contracts.Protocol
{
    using System;

    public sealed class Heartbeat : ProtocolEvent<Heartbeat>
    {
        protected override byte[] SerializeInternal(Heartbeat @event) => Array.Empty<byte>();

        protected override Heartbeat DeserializeInternal(byte[] bytes) => new Heartbeat();
    }
}