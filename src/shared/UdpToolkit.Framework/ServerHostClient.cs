namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Serialization;

    public sealed class ServerHostClient : IServerHostClient
    {
        private readonly TimeSpan _connectionTimeoutFromSettings;
        private readonly TimeSpan _resendPacketsTimeout;

        private readonly ITimersPool _timersPool;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly string _clientHost;
        private readonly List<int> _inputPorts;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly IServerSelector _serverSelector;
        private readonly ISerializer _serializer;
        private readonly IPeerManager _peerManager;

        public ServerHostClient(
            string clientHost,
            List<int> inputPorts,
            IAsyncQueue<NetworkPacket> outputQueue,
            IServerSelector serverSelector,
            ISerializer serializer,
            IDateTimeProvider dateTimeProvider,
            ITimersPool timersPool,
            TimeSpan connectionTimeout,
            TimeSpan resendPacketsTimeout,
            IPeerManager peerManager)
        {
            _clientHost = clientHost;
            _inputPorts = inputPorts;
            _outputQueue = outputQueue;
            _serverSelector = serverSelector;
            _serializer = serializer;
            _dateTimeProvider = dateTimeProvider;
            _timersPool = timersPool;
            _connectionTimeoutFromSettings = connectionTimeout;
            _resendPacketsTimeout = resendPacketsTimeout;
            _peerManager = peerManager;
        }

        public bool IsConnected { get; internal set; }

        internal Guid PeerId { get; set; } = Guid.NewGuid();

        public bool Connect(
            TimeSpan? connectionTimeout = null)
        {
            var timeout = connectionTimeout ?? _connectionTimeoutFromSettings;
            var @event = new Connect(PeerId, _clientHost, _inputPorts);
            var serverIp = _serverSelector.GetServer();
            var ips = _inputPorts.Select(port => new IPEndPoint(IPAddress.Parse(_clientHost), port)).ToList();
            var peer = _peerManager.AddOrUpdate(
                peerId: PeerId,
                ips: ips);

            PublishInternal(
                resendTimeout: timeout,
                @event: @event,
                ipEndPoint: serverIp.GetRandomIp(),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Connect,
                udpMode: UdpMode.ReliableUdp,
                serializer: _serializer.SerializeContractLess);

            return SpinWait.SpinUntil(() => IsConnected, timeout * 1.2);
        }

        public bool Disconnect()
        {
            var timeout = _resendPacketsTimeout;
            var serverIp = _serverSelector.GetServer();
            var @event = new Disconnect(peerId: PeerId);

            PublishInternal(
                resendTimeout: timeout,
                @event: @event,
                ipEndPoint: serverIp.GetRandomIp(),
                packetType: PacketType.Protocol,
                hookId: (byte)ProtocolHookId.Disconnect,
                udpMode: UdpMode.ReliableUdp,
                serializer: _serializer.SerializeContractLess);

            return SpinWait.SpinUntil(() => !IsConnected, timeout * 1.2);
        }

        public void Publish<TEvent>(
            TEvent @event,
            byte hookId,
            UdpMode udpMode)
        {
            var serverIp = _serverSelector.GetServer();

            PublishInternal(
                packetType: PacketType.UserDefined,
                resendTimeout: _resendPacketsTimeout,
                @event: @event,
                ipEndPoint: serverIp.GetRandomIp(),
                hookId: hookId,
                udpMode: udpMode,
                serializer: _serializer.Serialize);
        }

        public void PublishP2P<TEvent>(
            TEvent @event,
            IPEndPoint ipEndPoint,
            byte hookId,
            UdpMode udpMode)
        {
            PublishInternal(
                packetType: PacketType.UserDefined,
                resendTimeout: _resendPacketsTimeout,
                @event: @event,
                ipEndPoint: ipEndPoint,
                hookId: hookId,
                udpMode: udpMode,
                serializer: _serializer.Serialize);
        }

        private void PublishInternal<TEvent>(
            TEvent @event,
            IPEndPoint ipEndPoint,
            TimeSpan resendTimeout,
            byte hookId,
            UdpMode udpMode,
            PacketType packetType,
            Func<TEvent, byte[]> serializer)
        {
            _outputQueue.Produce(
                @event: new NetworkPacket(
                    networkPacketType: (NetworkPacketType)(byte)packetType,
                    createdAt: _dateTimeProvider.UtcNow(),
                    resendTimeout: resendTimeout,
                    channelType: udpMode.Map(),
                    peerId: PeerId,
                    channelHeader: default,
                    serializer: () => serializer(@event),
                    ipEndPoint: ipEndPoint,
                    hookId: hookId));
        }
    }
}
