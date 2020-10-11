namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Serialization;

    public sealed class ServerHostClient : IServerHostClient
    {
        private readonly TimeSpan _connectionTimeoutFromSettings;
        private readonly TimeSpan _resendPacketsTimeout;

        private readonly int? _pingDelayMs;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly string _clientHost;
        private readonly List<int> _inputPorts;
        private readonly IServerSelector _serverSelector;
        private readonly ISerializer _serializer;
        private readonly IPeerManager _peerManager;
        private readonly IBroadcastStrategyResolver _broadcastStrategyResolver;
        private readonly Task _ping;
        private readonly CancellationTokenSource _cts;

        public ServerHostClient(
            string clientHost,
            List<int> inputPorts,
            IServerSelector serverSelector,
            ISerializer serializer,
            IDateTimeProvider dateTimeProvider,
            TimeSpan connectionTimeout,
            TimeSpan resendPacketsTimeout,
            IPeerManager peerManager,
            IBroadcastStrategyResolver broadcastStrategyResolver,
            int? pingDelayMs)
        {
            _clientHost = clientHost;
            _inputPorts = inputPorts;
            _serverSelector = serverSelector;
            _serializer = serializer;
            _dateTimeProvider = dateTimeProvider;
            _connectionTimeoutFromSettings = connectionTimeout;
            _resendPacketsTimeout = resendPacketsTimeout;
            _peerManager = peerManager;
            _broadcastStrategyResolver = broadcastStrategyResolver;
            _pingDelayMs = pingDelayMs;
            _cts = new CancellationTokenSource();
            _ping = Task.Run(() => PingHost(_cts.Token));
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

            _cts.Cancel();

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

        private void PublishInternal<TEvent>(
            TEvent @event,
            IPEndPoint ipEndPoint,
            TimeSpan resendTimeout,
            byte hookId,
            UdpMode udpMode,
            PacketType packetType,
            Func<TEvent, byte[]> serializer)
        {
            _broadcastStrategyResolver
                .Resolve(
                    broadcastType: BroadcastType.Server)
                .Execute(
                    roomId: ushort.MaxValue,
                    networkPacket: new NetworkPacket(
                        networkPacketType: packetType.Map(),
                        createdAt: _dateTimeProvider.UtcNow(),
                        resendTimeout: resendTimeout,
                        channelType: udpMode.Map(),
                        peerId: PeerId,
                        channelHeader: default,
                        serializer: () => serializer(@event),
                        ipEndPoint: ipEndPoint,
                        hookId: hookId));
        }

        private async Task PingHost(
            CancellationToken token)
        {
            if (!_pingDelayMs.HasValue)
            {
                return;
            }

            var pingDelay = _pingDelayMs.Value;
            while (!token.IsCancellationRequested)
            {
                if (IsConnected)
                {
                    var serverIp = _serverSelector.GetServer();
                    var @event = new Ping();

                    PublishInternal(
                        resendTimeout: _resendPacketsTimeout,
                        @event: @event,
                        ipEndPoint: serverIp.GetRandomIp(),
                        packetType: PacketType.Protocol,
                        hookId: (byte)ProtocolHookId.Ping,
                        udpMode: UdpMode.ReliableUdp,
                        serializer: _serializer.SerializeContractLess);
                }

                await Task.Delay(pingDelay, token).ConfigureAwait(false);
            }
        }
    }
}
