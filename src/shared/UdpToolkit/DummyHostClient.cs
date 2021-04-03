namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;

    public sealed class DummyHostClient : IHostClient
    {
#pragma warning disable CS0067
        public event Action OnConnectionTimeout;
#pragma warning restore CS0067

        public Guid ConnectionId => Guid.Empty;

        public TimeSpan Rtt => TimeSpan.Zero;

        public bool IsConnected => false;

        public void Connect()
        {
        }

        public void Disconnect()
        {
        }

        public void Send<TEvent>(TEvent @event, byte hookId, UdpMode udpMode)
        {
        }
    }
}