namespace UdpToolkit.Framework
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Core;

    public interface IRawRoomManager
    {
        void Remove(
            int roomId,
            Peer peer);

        Task Apply(
            int roomId,
            Guid caller,
            Func<Peer, bool> condition,
            Func<Peer, Task> func);
    }
}