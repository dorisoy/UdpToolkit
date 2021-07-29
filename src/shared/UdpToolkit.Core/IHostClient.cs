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

        void ConnectToPeer(
            string host,
            int port);

        void Disconnect();

        void Send<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode);

        public void Send<TEvent>(
            TEvent @event,
            byte hookId,
            IpV4Address destination,
            UdpMode udpMode);
    }
}