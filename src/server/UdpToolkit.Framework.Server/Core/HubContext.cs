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
            ushort roomId,
            UdpMode udpMode)
        {
            PeerId = peerId;
            HubId = hubId;
            RpcId = rpcId;
            RoomId = roomId;
            UdpMode = udpMode;
        }

        public byte HubId { get; }

        public byte RpcId { get; }

        public ushort RoomId { get; }

        public UdpMode UdpMode { get; }

        public Guid PeerId { get; }
    }
}