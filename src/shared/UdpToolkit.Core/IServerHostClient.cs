namespace UdpToolkit.Core
{
    using System;
    using System.Net;

    public interface IServerHostClient
    {
        bool IsConnected { get; }

        bool Connect(TimeSpan? connectionTimeout = null);

        bool Disconnect();

        void Publish<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode);
    }
}