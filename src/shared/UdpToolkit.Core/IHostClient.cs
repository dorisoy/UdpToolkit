namespace UdpToolkit.Core
{
    using System;

    public interface IHostClient
    {
        Guid ConnectionId { get; }

        TimeSpan Rtt { get; }

        bool IsConnected { get; }

        bool Connect(
            TimeSpan? connectionTimeout = null);

        void ConnectAsync(
            TimeSpan? connectionTimeout = null);

        bool Disconnect();

        void Send<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode);
    }
}