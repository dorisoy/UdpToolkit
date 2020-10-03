namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Serialization;

    public sealed class Host : IHost
    {
        private readonly ILogger _logger = Log.ForContext<Host>();

        private readonly int _workers;
        private readonly int? _pingDelayMs;
        private readonly TimeSpan _resendPacketsTimeout;

        private readonly ISerializer _serializer;

        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly IAsyncQueue<NetworkPacket> _inputQueue;

        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly IRawPeerManager _rawPeerManager;
        private readonly IPeerManager _peerManager;
        private readonly IDatagramBuilder _datagramBuilder;

        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IProtocolSubscriptionManager _protocolSubscriptionManager;

        private readonly IRoomManager _roomManager;
        private readonly IServerSelector _serverSelector;
        private readonly ServerHostClient _serverHostClient;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITimersPool _timersPool;

        public Host(
            int workers,
            int? pingDelayMs,
            TimeSpan resendPacketsTimeout,
            ISerializer serializer,
            IAsyncQueue<NetworkPacket> outputQueue,
            IAsyncQueue<NetworkPacket> inputQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            ISubscriptionManager subscriptionManager,
            IPeerManager peerManager,
            IRawPeerManager rawPeerManager,
            IDatagramBuilder datagramBuilder,
            IRoomManager roomManager,
            IServerSelector serverSelector,
            IProtocolSubscriptionManager protocolSubscriptionManager,
            ServerHostClient serverHostClient,
            IDateTimeProvider dateTimeProvider,
            ITimersPool timersPool)
        {
            _workers = workers;
            _pingDelayMs = pingDelayMs;
            _resendPacketsTimeout = resendPacketsTimeout;
            _serializer = serializer;
            _outputQueue = outputQueue;
            _senders = senders;
            _receivers = receivers;
            _subscriptionManager = subscriptionManager;
            _peerManager = peerManager;
            _datagramBuilder = datagramBuilder;
            _serverHostClient = serverHostClient;
            _dateTimeProvider = dateTimeProvider;
            _timersPool = timersPool;
            _rawPeerManager = rawPeerManager;
            _roomManager = roomManager;
            _serverSelector = serverSelector;
            _protocolSubscriptionManager = protocolSubscriptionManager;
            _inputQueue = inputQueue;

            foreach (var receiver in _receivers)
            {
                receiver.UdpPacketReceived += OnUdpPacketReceived;
            }
        }

        public IServerHostClient ServerHostClient => _serverHostClient;

        public async Task RunAsync()
        {
            var senders = _senders
                .Select(
                    sender => TaskUtils.RunWithRestartOnFail(
                        job: () => StartSenderAsync(sender),
                        logger: (exception) =>
                        {
                            _logger.Error("Exception on send task: {@Exception}", exception);
                            _logger.Warning("Restart sender...");
                        },
                        token: default))
                .ToList();

            var receivers = _receivers
                .Select(
                    receiver => TaskUtils.RunWithRestartOnFail(
                        job: () => StartReceiverAsync(receiver),
                        logger: (exception) =>
                        {
                            _logger.Error("Exception on receive task: {@Exception}", exception);
                            _logger.Warning("Restart receiver...");
                        },
                        token: default))
                .ToList();

            var ping = TaskUtils.RunWithRestartOnFail(
                job: () => StartPingHost(),
                logger: (exception) =>
                {
                    _logger.Error("Exception on ping task: {@Exception}", exception);
                    _logger.Warning("Restart ping tak...");
                },
                token: default);

            var workers = Enumerable
                .Range(0, _workers)
                .Select(_ => Task.Run(StartWorkerAsync))
                .ToList();

            var tasks = senders
                .Concat(receivers)
                .Concat(workers)
                .Concat(new[] { ping });

            _logger.Information($"{nameof(Host)} running...");

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);
        }

        public void Stop()
        {
            _inputQueue.Stop();
            _outputQueue.Stop();
        }

        public void OnCore<TEvent>(Subscription subscription, byte hookId)
        {
            _subscriptionManager.Subscribe<TEvent>(hookId, subscription);
        }

        public void PublishCore<TEvent>(
            Func<IDatagramBuilder, Datagram<TEvent>> datagramFactory,
            UdpMode udpMode)
        {
            var datagram = datagramFactory(_datagramBuilder);

            foreach (var peer in datagram.Peers)
            {
                _outputQueue.Produce(
                    @event: new NetworkPacket(
                        createdAt: _dateTimeProvider.UtcNow(),
                        noAckCallback: () => { },
                        resendTimeout: _resendPacketsTimeout,
                        channelHeader: default,
                        serializer: () => _serializer.Serialize(datagram.Event),
                        ipEndPoint: peer.IpEndPoint,
                        hookId: datagram.HookId,
                        channelType: udpMode.Map(),
                        peerId: peer.PeerId));
            }
        }

        public void PublishInternal<TEvent>(
            Datagram<TEvent> datagram,
            UdpMode udpMode,
            Func<TEvent, byte[]> serializer)
        {
            foreach (var peer in datagram.Peers)
            {
                _outputQueue.Produce(
                    @event: new NetworkPacket(
                        createdAt: _dateTimeProvider.UtcNow(),
                        noAckCallback: () => { },
                        resendTimeout: _resendPacketsTimeout,
                        channelHeader: default,
                        serializer: () => serializer(datagram.Event),
                        ipEndPoint: peer.IpEndPoint,
                        hookId: datagram.HookId,
                        channelType: udpMode.Map(),
                        peerId: peer.PeerId));
            }
        }

        public void Dispose()
        {
            foreach (var sender in _senders)
            {
                sender.Dispose();
            }

            foreach (var receiver in _receivers)
            {
                receiver.Dispose();
            }

            _inputQueue.Dispose();
            _outputQueue.Dispose();
        }

        private async Task StartPingHost(
            CancellationToken token = default)
        {
            if (!_pingDelayMs.HasValue)
            {
                return;
            }

            var pingDelay = _pingDelayMs.Value;
            while (!token.IsCancellationRequested)
            {
                if (_serverHostClient.IsConnected)
                {
                    _outputQueue.Produce(@event: new NetworkPacket(
                        createdAt: _dateTimeProvider.UtcNow(),
                        noAckCallback: () => { },
                        resendTimeout: TimeSpan.FromSeconds(pingDelay * 1.2),
                        channelType: ChannelType.ReliableUdp,
                        peerId: _serverHostClient.PeerId,
                        channelHeader: default,
                        serializer: () => _serializer.SerializeContractLess(@event: new Ping()),
                        ipEndPoint: _serverSelector.GetServer(),
                        hookId: (byte)ProtocolHookId.Ping));
                }

                await Task.Delay(pingDelay, token).ConfigureAwait(false);
            }
        }

        private void StartWorkerAsync()
        {
            foreach (var networkPacket in _inputQueue.Consume())
            {
                try
                {
                    ProcessPacketAsync(networkPacket: networkPacket);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Unhandled exception on process packet, {@Exception}", ex);
                }
            }
        }

        private async Task StartReceiverAsync(IUdpReceiver udpReceiver)
        {
            await udpReceiver
                .StartReceiveAsync()
                .ConfigureAwait(false);
        }

        private async Task StartSenderAsync(IUdpSender udpSender)
        {
            foreach (var networkPacket in _outputQueue.Consume())
            {
                if (networkPacket.ProtocolHookId < ProtocolHookId.Ping)
                {
                    ProcessUserDefinedOutputEvent(networkPacket: networkPacket);
                }
                else
                {
                    ProcessProtocolOutputEvent(networkPacket: networkPacket);
                }

                await Task.CompletedTask.ConfigureAwait(false);

                var exists = _rawPeerManager.TryGetPeer(peerId: networkPacket.PeerId, out var peer);
                if (!exists)
                {
                    continue;
                }

                var packet = peer
                    .GetChannel(networkPacket.ChannelType)
                    .TryHandleOutputPacket(networkPacket: networkPacket);

                if (packet != null)
                {
                    await udpSender
                        .SendAsync(packet)
                        .ConfigureAwait(false);
                }
            }
        }

        private void ProcessProtocolOutputEvent(NetworkPacket networkPacket)
        {
            var subscription = _protocolSubscriptionManager
                .GetOutputSubscription((byte)networkPacket.ProtocolHookId);

            switch (networkPacket.ProtocolHookId)
            {
                case ProtocolHookId.Connected when _serverHostClient.IsConnected:
                case ProtocolHookId.Disconnect when _serverHostClient.IsConnected:
                case ProtocolHookId.Disconnected when _serverHostClient.IsConnected:
                case ProtocolHookId.P2P when _serverHostClient.IsConnected:
                case ProtocolHookId.Ping when _serverHostClient.IsConnected:
                case ProtocolHookId.Pong when _serverHostClient.IsConnected:
                case ProtocolHookId.Ack when _serverHostClient.IsConnected:
                case ProtocolHookId.Connect:
                    subscription.Invoke(
                        bytes: networkPacket.Serializer(),
                        peerManager: _peerManager,
                        peerId: networkPacket.PeerId,
                        serializer: _serializer,
                        host: this,
                        datagramBuilder: _datagramBuilder,
                        dateTimeProvider: _dateTimeProvider,
                        timersPool: _timersPool);
                    break;
            }
        }

        private void ProcessProtocolInputEvent(NetworkPacket networkPacket)
        {
            _logger.Debug("Packet received: {@packet}", networkPacket);

            var protocolHookId = networkPacket.ProtocolHookId;
            var subscription = _protocolSubscriptionManager.GetInputSubscription((byte)protocolHookId);

            var peerExists = _rawPeerManager.TryGetPeer(networkPacket.PeerId, out var peer);

            switch (protocolHookId)
            {
                case ProtocolHookId.Ping when peerExists:
                case ProtocolHookId.Pong when peerExists:
                case ProtocolHookId.P2P when peerExists:
                case ProtocolHookId.Connected when peerExists:
                case ProtocolHookId.Disconnect when peerExists:
                case ProtocolHookId.Disconnected when peerExists:
                case ProtocolHookId.Connect:
                    peer
                        .GetChannel(ChannelType.ReliableUdp)
                        .TryHandleInputPacket(networkPacket);

                    break;
                case ProtocolHookId.Ack when peerExists:
                    switch (protocolHookId)
                    {
                        case ProtocolHookId.Connected:
                            _serverHostClient.IsConnected = true;
                            break;
                        case ProtocolHookId.Disconnected:
                            _serverHostClient.IsConnected = false;
                            break;
                    }

                    subscription.Invoke(
                        serializer: _serializer,
                        peerManager: _peerManager,
                        host: this,
                        datagramBuilder: _datagramBuilder,
                        bytes: networkPacket.Serializer(),
                        peerId: networkPacket.PeerId,
                        dateTimeProvider: _dateTimeProvider,
                        timersPool: _timersPool);

                    break;
            }
        }

        private void ProcessUserDefinedOutputEvent(NetworkPacket networkPacket)
        {
            // nothing to do right now
        }

        private void ProcessPacketAsync(NetworkPacket networkPacket)
        {
            var bytes = networkPacket.Serializer();
            var subscription = _subscriptionManager.GetSubscription(hookId: networkPacket.HookId);

            subscription(
                bytes: bytes,
                peerId: networkPacket.PeerId,
                serializer: _serializer,
                roomManager: _roomManager,
                datagramBuilder: _datagramBuilder,
                udpMode: networkPacket.ChannelType.Map());
        }

        private void OnUdpPacketReceived(NetworkPacket networkPacket)
        {
            if (networkPacket.ProtocolHookId < ProtocolHookId.Ping)
            {
                ProcessUserDefinedInputEvent(networkPacket: networkPacket);
            }
            else
            {
                ProcessProtocolInputEvent(networkPacket: networkPacket);
            }
        }

        private void ProcessUserDefinedInputEvent(NetworkPacket networkPacket)
        {
            _logger.Debug("Packet received: {@packet}", networkPacket);

            var peerExists = _rawPeerManager.TryGetPeer(peerId: networkPacket.PeerId, out var rawPeer);
            if (!peerExists)
            {
                return;
            }

            var channel = rawPeer.GetChannel(channelType: networkPacket.ChannelType);
            if (networkPacket.ProtocolHookId == ProtocolHookId.Ack)
            {
                var result = channel.HandleAck(networkPacket: networkPacket);

                if (result != null)
                {
                    _inputQueue.Produce(result);
                }
                else
                {
                    _logger.Debug($"Ack dropped! {networkPacket.ChannelHeader.Id}");
                }
            }
            else
            {
                var result = channel.TryHandleInputPacket(networkPacket: networkPacket);
                if (result.ChannelState == ChannelState.Accepted)
                {
                    _inputQueue.Produce(result.NetworkPacket);
                }
                else if (result.ChannelState == ChannelState.Resend)
                {
                    _outputQueue.Produce(result.NetworkPacket);
                }
                else
                {
                    _logger.Debug($"Input NetworkPacket dropped! {networkPacket.ChannelHeader.Id}");
                }
            }

            // channel.Resend();
        }
    }
}
