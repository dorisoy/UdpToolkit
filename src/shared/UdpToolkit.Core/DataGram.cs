namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public readonly struct DataGram<TResp>
    {
        public DataGram(
            TResp response,
            IEnumerable<ShortPeer> peers,
            byte hookId)
        {
            Response = response;
            Peers = peers;
            HookId = hookId;
        }

        public byte HookId { get; }

        public TResp Response { get; }

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