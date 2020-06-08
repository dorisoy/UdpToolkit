namespace UdpToolkit.Framework.Server.Core
{
    using System;
    using UdpToolkit.Core;

    public readonly struct HubContext
    {
        public HubContext(
            Guid peerId,
            byte hubId,
            byte rpcId,
            UdpMode udpMode)
        {
            PeerId = peerId;
            HubId = hubId;
            RpcId = rpcId;
            UdpMode = udpMode;
        }

        public byte HubId { get; }

        public byte RpcId { get; }

        public UdpMode UdpMode { get; }

        public Guid PeerId { get; }
    }
}