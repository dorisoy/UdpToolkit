namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public interface IHostClient : IDisposable
    {
        event Action<IpV4Address, Guid> OnDisconnected;

        event Action<IpV4Address, Guid> OnConnected;

        event Action OnConnectionTimeout;

        event Action<double> OnRttReceived;

        void Connect();

        void Connect(
            string host,
            int port);

        void Disconnect(
            string host,
            int port);

        void Disconnect();

        void Send<TEvent>(
            TEvent @event,
            byte channelId);

        public void Send<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId);
    }
}