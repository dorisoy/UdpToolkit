using System;
using System.Net;

namespace UdpToolkit.Network.Packets
{
    public readonly struct InputUdpPacket
    {
        public byte HubId { get; }
        
        public byte RpcId { get; }

        public ushort ScopeId { get; }
        
        public string PeerId { get; }

        public IPEndPoint RemotePeer { get; }

        public ArraySegment<byte> Payload { get; }

        public InputUdpPacket(
            byte hubId, 
            byte rpcId,
            ushort scopeId,
            string peerId,
            ArraySegment<byte> payload, 
            IPEndPoint remotePeer)
        {
            HubId = hubId;
            RpcId = rpcId;
            Payload = payload;
            RemotePeer = remotePeer;
            PeerId = peerId;
            ScopeId = scopeId;
        }
    }
}
