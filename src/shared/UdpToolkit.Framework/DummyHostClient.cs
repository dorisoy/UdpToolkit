namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Sockets;

    public sealed class DummyHostClient : IHostClient
    {
        public DummyHostClient()
        {
            OnDisconnected += (ipV4, connectionId) => { };
            OnConnected += (ipV4, connectionId) => { };
            OnRttReceived += rtt => { };
            OnConnectionTimeout += () => { };

            OnConnectionTimeout?.Invoke();
            OnConnected?.Invoke(default, default);
            OnDisconnected?.Invoke(default, default);
            OnRttReceived?.Invoke(default);
        }

        public event Action<IpV4Address, Guid> OnDisconnected;

        public event Action<IpV4Address, Guid> OnConnected;

        public event Action OnConnectionTimeout;

        public event Action<double> OnRttReceived;

        public void Connect()
        {
            // nothing to do
        }

        public void Connect(
            string host,
            int port)
        {
            // nothing to do
        }

        public void Disconnect(
            string host,
            int port)
        {
            // nothing to do
        }

        public void Disconnect()
        {
            // nothing to do
        }

        public void Send<TEvent>(
            TEvent @event,
            byte channelId)
        {
            // nothing to do
        }

        public void Send<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId)
        {
            // nothing to do
        }

        public void Dispose()
        {
            // nothing to do
        }
    }
}