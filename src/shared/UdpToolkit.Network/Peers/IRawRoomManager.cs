namespace UdpToolkit.Network.Peers
{
    using System;
    using System.Threading.Tasks;

    public interface IRawRoomManager
    {
        void Remove(
            int roomId,
            IRawPeer peer);

        Task Apply(
            int roomId,
            Guid caller,
            Func<IRawPeer, bool> condition,
            Func<IRawPeer, Task> func);
    }
}