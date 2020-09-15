namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public readonly struct Datagram<TEvent>
    {
        public Datagram(
            TEvent @event,
            IEnumerable<ShortPeer> peers,
            byte hookId)
        {
            Event = @event;
            Peers = peers;
            HookId = hookId;
        }

        public byte HookId { get; }

        public TEvent Event { get; }

        public IEnumerable<ShortPeer> Peers { get; }
    }

    public readonly struct ShortPeer
    {
        public ShortPeer(
            Guid peerId,
            IPEndPoint ipEndPoint)
        {
            PeerId = peerId;
            IpEndPoint = ipEndPoint;
        }

        public Guid PeerId { get; }

        public IPEndPoint IpEndPoint { get; }
    }
}