namespace UdpToolkit.Core
{
    using System;
    using UdpToolkit.Core.ProtocolEvents;

    public interface IProtocolSubscriptionManager
    {
        void OnConnect(Guid peerId, byte[] bytes);

        void OnConnected(Guid peerId, byte[] bytes);

        void OnDisconnect(Guid peerId, byte[] bytes);

        void OnDisconnected(Guid peerId, byte[] bytes);
    }
}