namespace UdpToolkit.Core
{
    using System;

    public interface IRoomManager : IDisposable
    {
        void JoinOrCreate(
            int roomId,
            Guid connectionId);

        Room GetRoom(
            int roomId);

        void Leave(
            int roomId,
            Guid connectionId);
    }
}