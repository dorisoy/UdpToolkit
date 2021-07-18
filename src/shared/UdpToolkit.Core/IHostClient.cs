namespace UdpToolkit.Core
{
    using System;

    public interface IHostClient : IDisposable
    {
        event Action OnConnectionTimeout;

        Guid ConnectionId { get; }

        bool IsConnected { get; }

        TimeSpan Rtt { get; }

        void Connect();

        void Disconnect();

        void Send<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode);
    }
}