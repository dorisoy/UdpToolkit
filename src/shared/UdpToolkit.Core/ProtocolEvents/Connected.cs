namespace UdpToolkit.Core.ProtocolEvents
{
    using System;

    public class Connected
    {
        public Connected(Guid peerId)
        {
            PeerId = peerId;
        }

        public Guid PeerId { get; }
    }
}