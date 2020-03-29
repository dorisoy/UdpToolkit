namespace UdpToolkit.Core
{
    using System.Collections.Generic;
    using System.Net;

    public readonly struct CallContext
    {
        public CallContext(
            byte hubId,
            byte rpcId,
            ushort scopeId,
            UdpMode udpMode,
            byte[] payload,
            IEnumerable<IPEndPoint> peerIPs)
        {
            Payload = payload;
            PeerIPs = peerIPs;
            HubId = hubId;
            RpcId = rpcId;
            ScopeId = scopeId;
            UdpMode = udpMode;
        }

        public byte HubId { get; }

        public byte RpcId { get; }

        public ushort ScopeId { get; }

        public UdpMode UdpMode { get; }

        public byte[] Payload { get; }

        public IEnumerable<IPEndPoint> PeerIPs { get; }
    }
}