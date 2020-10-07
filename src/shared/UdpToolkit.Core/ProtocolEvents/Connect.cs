namespace UdpToolkit.Core.ProtocolEvents
{
    using System;
    using System.Collections.Generic;

    public sealed class Connect : IProtocolEvent
    {
        public Connect(
            Guid peerId,
            string clientHost,
            List<int> clientIps)
        {
            PeerId = peerId;
            ClientHost = clientHost;
            ClientIps = clientIps;
        }

        public List<int> ClientIps { get; }

        public Guid PeerId { get; }

        public string ClientHost { get; }
    }
}