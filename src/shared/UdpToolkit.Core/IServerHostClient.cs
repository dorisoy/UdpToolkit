namespace UdpToolkit.Core
{
    using System;
    using System.Net;

    public interface IServerHostClient
    {
        bool IsConnected { get; }

        bool Connect();

        void Disconnect();

        void Publish<TEvent>(TEvent @event, byte hookId, UdpMode udpMode);

        void PublishP2P<TEvent>(TEvent @event, IPEndPoint ipEndPoint, byte hookId, UdpMode udpMode);
    }
}