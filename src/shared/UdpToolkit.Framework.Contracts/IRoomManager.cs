namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public interface IRoomManager : IDisposable
    {
        void JoinOrCreate(
            int roomId,
            Guid connectionId,
            IpV4Address ipV4Address);

        Room GetRoom(
            int roomId);

        void Leave(
            int roomId,
            Guid connectionId);
    }
}