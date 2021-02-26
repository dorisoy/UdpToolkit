namespace UdpToolkit.Network
{
    using System;
    using System.Threading.Tasks;

    public interface IRawRoomManager
    {
        void Remove(
            int roomId,
            IConnection connection);

        Task Apply(
            int roomId,
            Guid caller,
            Func<Guid, bool> condition,
            Func<Guid, Task> func);
    }
}