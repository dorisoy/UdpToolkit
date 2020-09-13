namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Serialization;

    public sealed class ProtocolSubscriptionManager : IProtocolSubscriptionManager
    {
        private readonly IPeerManager _peerManager;
        private readonly ISerializer _serializer;

        public ProtocolSubscriptionManager(
            IPeerManager peerManager,
            ISerializer serializer)
        {
            _peerManager = peerManager;
            _serializer = serializer;
        }

        public void OnConnect(Guid peerId, byte[] bytes)
        {
            var exists = _peerManager.Exist(peerId);
            if (exists)
            {
                return;
            }

            var connect = _serializer.DeserializeContractLess<Connect>(bytes);
            _peerManager.Create(peerId, connect.GetPeerIps());
        }

        public void OnConnected(Guid peerId, byte[] bytes)
        {
        }

        public void OnDisconnect(Guid peerId, byte[] bytes)
        {
            var disconnect = _serializer.DeserializeContractLess<Disconnect>(bytes);
            _peerManager.Remove(peerId);
        }

        public void OnDisconnected(Guid peerId, byte[] bytes)
        {
            var disconnected = _serializer.DeserializeContractLess<Disconnected>(bytes);
            _peerManager.Remove(peerId);
        }
    }
}