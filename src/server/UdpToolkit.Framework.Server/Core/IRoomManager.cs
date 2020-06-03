namespace UdpToolkit.Framework.Server.Core
{
    using System;

    public interface IRoomManager
    {
        void JoinOrCreate(ushort roomId, Guid peerId);

        void JoinOrCreate(ushort roomId, Guid peerId, int limit);

        IRoom GetRoom(ushort roomId);

        void Leave(ushort roomId, Guid peerId);
    }
}
