namespace UdpToolkit.Core.ProtocolEvents
{
    using System;

    public sealed class Disconnected
    {
        public Disconnected(Guid peerId)
        {
            PeerId = peerId;
        }

        public Guid PeerId { get; }
    }
}