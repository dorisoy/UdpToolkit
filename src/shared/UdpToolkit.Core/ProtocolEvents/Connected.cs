namespace UdpToolkit.Core.ProtocolEvents
{
    using System;
    using System.Collections.Generic;

    public class Connected
    {
        public Connected(Guid peerId)
        {
            PeerId = peerId;
        }

        public Guid PeerId { get; }
    }
}