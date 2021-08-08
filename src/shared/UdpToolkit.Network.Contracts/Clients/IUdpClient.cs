namespace UdpToolkit.Network.Contracts.Clients
{
    using System;
    using System.Threading;
    using UdpToolkit.Network.Contracts.Packets;

    public interface IUdpClient : IDisposable
    {
        event Action<InPacket> OnPacketReceived;

        void Send(
            OutPacket outPacket);

        void Receive(
            CancellationToken cancellationToken);
    }
}