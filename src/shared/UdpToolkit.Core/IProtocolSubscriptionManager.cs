namespace UdpToolkit.Core
{
    using System;

    public interface IProtocolSubscriptionManager
    {
        void OnPong(Guid peerId, byte[] bytes, IHost host);

        void OnPing(Guid peerId, byte[] bytes, IHost host);

        void OnConnect(Guid peerId, byte[] bytes, IHost host);

        void OnConnected(Guid peerId, byte[] bytes);

        void OnDisconnect(Guid peerId, byte[] bytes, IHost host);

        void OnDisconnected(Guid peerId, byte[] bytes);
    }
}