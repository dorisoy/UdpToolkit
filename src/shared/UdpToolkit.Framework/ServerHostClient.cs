namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
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
        private readonly ITimersPool _timersPool;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly IServerSelector _serverSelector;
        private readonly List<int> _ips;
        private readonly ISerializer _serializer;

        public ServerHostClient(
            IAsyncQueue<NetworkPacket> outputQueue,
            IServerSelector serverSelector,
            List<int> ips,
            ISerializer serializer,
            IDateTimeProvider dateTimeProvider,
            ITimersPool timersPool,
            TimeSpan connectionTimeout,
            TimeSpan resendPacketsTimeout)
        {
            _outputQueue = outputQueue;
            _serverSelector = serverSelector;
            _ips = ips;
            _serializer = serializer;
            _dateTimeProvider = dateTimeProvider;
            _timersPool = timersPool;
            _connectionTimeoutFromSettings = connectionTimeout;
            _resendPacketsTimeout = resendPacketsTimeout;
        }

        public bool IsConnected { get; internal set; }

        internal Guid PeerId { get; set; }

        public bool Connect(TimeSpan? connectionTimeout = null)
        {
            var timeout = connectionTimeout ?? _connectionTimeoutFromSettings;
            var hostAddress = _serverSelector.GetServer().Address.ToString();
            var @event = new Connect(hostAddress, _ips);
            var serverIp = _serverSelector.GetServer();

            PublishInternal(
                noAckCallback: () => { _timersPool.DisableResend(PeerId); },
                resendTimeout: timeout,
                @event: @event,
                ipEndPoint: serverIp,
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
                noAckCallback: () => { _timersPool.DisableResend(PeerId); },
                resendTimeout: timeout,
                @event: @event,
                ipEndPoint: serverIp,
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
                noAckCallback: () => { },
                resendTimeout: _resendPacketsTimeout,
                @event: @event,
                ipEndPoint: serverIp,
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
                noAckCallback: () => { },
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
            Action noAckCallback,
            byte hookId,
            UdpMode udpMode,
            Func<TEvent, byte[]> serializer)
        {
            _outputQueue.Produce(
                @event: new NetworkPacket(
                    createdAt: _dateTimeProvider.UtcNow(),
                    resendTimeout: resendTimeout,
                    noAckCallback: noAckCallback,
                    channelType: udpMode.Map(),
                    peerId: PeerId,
                    channelHeader: default,
                    serializer: () => serializer(@event),
                    ipEndPoint: ipEndPoint,
                    hookId: hookId));
        }
    }
}
