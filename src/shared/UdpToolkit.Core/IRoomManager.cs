namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;

    public interface IRoomManager
    {
        void JoinOrCreate(
            int roomId,
            Guid peerId);

        IEnumerable<Guid> GetRoomPeers(
            int roomId);

        void Leave(
            int roomId,
            Guid peerId);
    }
}