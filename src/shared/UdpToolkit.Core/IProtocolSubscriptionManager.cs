namespace UdpToolkit.Core
{
    using System;

    public interface IProtocolSubscriptionManager
    {
        void OnConnect(Guid peerId, byte[] bytes, IHost host);

        void OnConnected(Guid peerId, byte[] bytes);

        void OnDisconnect(Guid peerId, byte[] bytes, IHost host);

        void OnDisconnected(Guid peerId, byte[] bytes);
    }
}