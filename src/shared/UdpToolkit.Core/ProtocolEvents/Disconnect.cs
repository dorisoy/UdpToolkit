namespace UdpToolkit.Core.ProtocolEvents
{
    using System;

    public class Disconnect : IProtocolEvent
    {
        public Disconnect(Guid peerId)
        {
            PeerId = peerId;
        }

        public Guid PeerId { get; }
    }
}