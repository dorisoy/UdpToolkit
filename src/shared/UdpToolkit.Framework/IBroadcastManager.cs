namespace UdpToolkit.Framework
{
    using System.Threading.Tasks;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;

    public interface IBroadcastManager
    {
        Task AckToServer(
            IUdpSender udpSender,
            NetworkPacket networkPacket);

        Task Caller(
            IUdpSender udpSender,
            NetworkPacket networkPacket);

        Task Room(
            int roomId,
            IUdpSender udpSender,
            NetworkPacket networkPacket);

        Task RoomExceptCaller(
            int roomId,
            IUdpSender udpSender,
            NetworkPacket networkPacket);

        Task AllServer(
            IUdpSender udpSender,
            NetworkPacket networkPacket);

        Task Server(
            IUdpSender udpSender,
            NetworkPacket networkPacket);
    }
}