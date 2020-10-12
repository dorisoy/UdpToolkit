namespace UdpToolkit.Core
{
    using System;

    public interface IServerHostClient
    {
        bool IsConnected { get; }

        bool Connect(
            TimeSpan? connectionTimeout = null);

        bool Disconnect();

        void Publish<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode);
    }
}