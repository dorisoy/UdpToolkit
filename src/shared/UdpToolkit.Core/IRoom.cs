namespace UdpToolkit.Core
{
    using System;

    public interface IRoom
    {
        ushort RoomId { get; }

        void AddPeer(IPeer peer);

        void RemovePeer(Guid peerId);
    }
}