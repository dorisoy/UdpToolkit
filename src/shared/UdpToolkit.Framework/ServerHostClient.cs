namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Serialization;

    public sealed class ServerHostClient : IServerHostClient
    {
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly IServerSelector _serverSelector;
        private readonly List<int> _ips;
        private readonly ISerializer _serializer;

        public ServerHostClient(
            IAsyncQueue<NetworkPacket> outputQueue,
            IServerSelector serverSelector,
            List<int> ips,
            ISerializer serializer,
            Guid me)
        {
            _outputQueue = outputQueue;
            _serverSelector = serverSelector;
            _ips = ips;
            _serializer = serializer;
            Me = me;
        }

        public Guid Me { get; }

        public void Connect()
        {
            var host = _serverSelector.GetServer().Address.ToString();
            var @event = new Connect(host, _ips);
            var serverIp = _serverSelector.GetServer();

            PublishInternal(
                @event: @event,
                ipEndPoint: serverIp,
                hookId: (byte)PacketType.Connect,
                udpMode: UdpMode.ReliableUdp,
                serializer: _serializer.SerializeContractLess);
        }

        public void Disconnect()
        {
            var serverIp = _serverSelector.GetServer();
            var @event = new Disconnect();

            PublishInternal(
                @event: @event,
                ipEndPoint: serverIp,
                hookId: (byte)PacketType.Disconnect,
                udpMode: UdpMode.ReliableUdp,
                serializer: _serializer.SerializeContractLess);
        }

        public void Publish<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode)
        {
            var serverIp = _serverSelector.GetServer();

            PublishInternal(
                @event: @event,
                ipEndPoint: serverIp,
                hookId: hookId,
                udpMode: udpMode,
                _serializer.Serialize);
        }

        public void PublishP2P<TEvent>(
            TEvent @event,
            IPEndPoint ipEndPoint,
            byte hookId,
            UdpMode udpMode)
        {
            PublishInternal(
                @event: @event,
                ipEndPoint: ipEndPoint,
                hookId: hookId,
                udpMode: udpMode,
                _serializer.Serialize);
        }

        private void PublishInternal<TEvent>(
            TEvent @event,
            IPEndPoint ipEndPoint,
            byte hookId,
            UdpMode udpMode,
            Func<TEvent, byte[]> serializer)
        {
            _outputQueue.Produce(@event: new NetworkPacket(
                channelType: udpMode.Map(),
                peerId: Me,
                channelHeader: default,
                serializer: () => serializer(@event),
                ipEndPoint: ipEndPoint,
                hookId: hookId));
        }
    }
}
