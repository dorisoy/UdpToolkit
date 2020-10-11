namespace UdpToolkit.Core
{
    using System;

    public interface IRoomManager
    {
        void JoinOrCreate(ushort roomId, Guid peerId);

        IRoom GetRoom(ushort roomId);

        void Leave(ushort roomId, Guid peerId);
    }
}