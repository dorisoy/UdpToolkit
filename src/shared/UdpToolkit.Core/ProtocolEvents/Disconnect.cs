namespace UdpToolkit.Core.ProtocolEvents
{
    using System;

    public class Disconnect
    {
        public Disconnect(Guid peerId)
        {
            PeerId = peerId;
        }

        public Guid PeerId { get; }
    }
}