using System;

namespace UdpToolkit.Core
{
    public struct InputUdpPacket
    {
        public byte HubId { get; }
        
        public byte RpcId { get; }

        public ushort ScopeId { get; }
        
        public string PeerId { get; }

        public ArraySegment<byte> Request { get; }

        public InputUdpPacket(
            byte hubId, 
            byte rpcId,
            ushort scopeId,
            string peerId,
            ArraySegment<byte> request)
        {
            HubId = hubId;
            RpcId = rpcId;
            Request = request;
            PeerId = peerId;
            ScopeId = scopeId;
        }
    }
}
