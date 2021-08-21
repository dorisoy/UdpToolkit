namespace UdpToolkit.Network.Contracts.Clients
{
    using System;
    using System.Threading;
    using UdpToolkit.Network.Contracts.Sockets;

    public interface IUdpClient : IDisposable
    {
        event Action<IpV4Address, Guid, byte[], byte> OnPacketReceived;

        event Action<IpV4Address, Guid, byte[], byte> OnPacketExpired;

        event Action<IpV4Address, Guid> OnConnected;

        event Action<IpV4Address, Guid> OnDisconnected;

        event Action<Guid, TimeSpan> OnHeartbeat;

        bool IsConnected(
            out Guid connectionId);

        void Connect(
            IpV4Address ipV4Address);

        void Heartbeat(
            IpV4Address ipV4Address);

        void Disconnect(
            IpV4Address ipV4Address);

        void Send(
            Guid connectionId,
            byte channelId,
            byte[] bytes,
            IpV4Address destination);

        void StartReceive(
            CancellationToken cancellationToken);
    }
}