namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;

    public sealed class DummyHostClient : IHostClient
    {
        public DummyHostClient()
        {
            OnConnectionTimeout += () => { };
            OnConnectionTimeout?.Invoke();
        }

        public event Action OnConnectionTimeout;

        public Guid ConnectionId => Guid.Empty;

        public TimeSpan Rtt => TimeSpan.Zero;

        public bool IsConnected => false;

        public void Connect()
        {
            // nothing to do
        }

        public void ConnectToPeer(
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
            byte hookId,
            UdpMode udpMode)
        {
            // nothing to do
        }

        public void Send<TEvent>(
            TEvent @event,
            byte hookId,
            IpV4Address destination,
            UdpMode udpMode)
        {
            // nothing to do
        }

        public void Dispose()
        {
            // nothing to do
        }
    }
}