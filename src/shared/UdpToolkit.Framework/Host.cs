namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class Host : IHost
    {
        private readonly ILogger _logger = Log.ForContext<Host>();

        private readonly HostSettings _hostSettings;

        private readonly IAsyncQueue<CallContext> _outputQueue;
        private readonly IAsyncQueue<CallContext> _inputQueue;
        private readonly IAsyncQueue<CallContext> _resendQueue;

        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;
        private readonly IEnumerable<IUdpSender> _resenders;

        private readonly IRawPeerManager _rawPeerManager;

        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IProtocolSubscriptionManager _protocolSubscriptionManager;
        private readonly IBroadcastManager _broadcastManager;

        private readonly IRoomManager _roomManager;
        private readonly ServerHostClient _serverHostClient;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IScheduler _scheduler;
        private readonly TimersPool _timersPool;

        public Host(
            HostSettings hostSettings,
            IAsyncQueue<CallContext> outputQueue,
            IAsyncQueue<CallContext> inputQueue,
            IAsyncQueue<CallContext> resendQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            IEnumerable<IUdpSender> resenders,
            ISubscriptionManager subscriptionManager,
            IRawPeerManager rawPeerManager,
            IRoomManager roomManager,
            IProtocolSubscriptionManager protocolSubscriptionManager,
            ServerHostClient serverHostClient,
            IDateTimeProvider dateTimeProvider,
            IScheduler scheduler,
            TimersPool timersPool,
            IBroadcastManager broadcastManager)
        {
            _hostSettings = hostSettings;
            _outputQueue = outputQueue;
            _senders = senders;
            _receivers = receivers;
            _resenders = resenders;
            _subscriptionManager = subscriptionManager;
            _serverHostClient = serverHostClient;
            _dateTimeProvider = dateTimeProvider;
            _scheduler = scheduler;
            _timersPool = timersPool;
            _broadcastManager = broadcastManager;
            _rawPeerManager = rawPeerManager;
            _roomManager = roomManager;
            _protocolSubscriptionManager = protocolSubscriptionManager;
            _inputQueue = inputQueue;
            _resendQueue = resendQueue;

            foreach (var receiver in _receivers)
            {
                receiver.UdpPacketReceived += (networkPacket) =>
                {
                    _inputQueue.Produce(new CallContext(
                        roomId: null,
                        broadcastMode: null,
                        networkPacket: networkPacket,
                        resendTimeout: _hostSettings.ResendPacketsTimeout,
                        createdAt: _dateTimeProvider.UtcNow()));
                };
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

            var resenders = _resenders
                .Select(resender => TaskUtils.RunWithRestartOnFail(
                    job: () => StartResendAsync(resender),
                    logger: (exception) =>
                    {
                        _logger.Error("Exception on resend task: {@Exception}", exception);
                        _logger.Warning("Restart resender...");
                    },
                    token: default))
                .ToList();

            var workers = Enumerable
                .Range(0, _hostSettings.Workers)
                .Select(_ => Task.Run(StartWorkerAsync))
                .ToList();

            var tasks = senders
                .Concat(receivers)
                .Concat(workers)
                .Concat(resenders);

            _logger.Information($"{nameof(Host)} running...");

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);
        }

        public void Stop()
        {
            _inputQueue.Stop();
            _outputQueue.Stop();
            _timersPool.Dispose();
        }

        public void OnCore(Subscription subscription, byte hookId)
        {
            _subscriptionManager.Subscribe(hookId, subscription);
        }

        public void PublishCore<TEvent>(
            TEvent @event,
            int roomId,
            byte hookId,
            UdpMode udpMode)
        {
            _outputQueue.Produce(new CallContext(
                roomId: roomId,
                networkPacket: new NetworkPacket(
                    id: default,
                    acks: default,
                    ipEndPoint: null,
                    networkPacketType: NetworkPacketType.FromServer,
                    createdAt: _dateTimeProvider.UtcNow(),
                    channelType: udpMode.Map(),
                    peerId: default,
                    serializer: () => _hostSettings.Serializer.Serialize(@event),
                    hookId: hookId),
                broadcastMode: BroadcastMode.Room,
                resendTimeout: _hostSettings.ResendPacketsTimeout,
                createdAt: _dateTimeProvider.UtcNow()));
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

        private async Task StartResendAsync(IUdpSender udpSender)
        {
            foreach (var callContext in _resendQueue.Consume())
            {
                await udpSender
                    .SendAsync(callContext.NetworkPacket)
                    .ConfigureAwait(false);
            }
        }

        private void StartWorkerAsync()
        {
            foreach (var callContext in _inputQueue.Consume())
            {
                try
                {
                    switch (callContext.NetworkPacket.NetworkPacketType)
                    {
                        case NetworkPacketType.FromServer:
                            ProcessServerInputEvent(networkPacket: callContext.NetworkPacket);
                            break;
                        case NetworkPacketType.UserDefined:
                            ProcessUserDefinedInputEvent(networkPacket: callContext.NetworkPacket);
                            break;
                        case NetworkPacketType.Protocol:
                            ProcessProtocolInputEvent(networkPacket: callContext.NetworkPacket);
                            break;
                        case NetworkPacketType.Ack:
                            if (callContext.NetworkPacket.IsProtocolEvent)
                            {
                                ProcessInputProtocolAck(networkPacket: callContext.NetworkPacket);
                            }
                            else
                            {
                                ProcessInputAck(networkPacket: callContext.NetworkPacket);
                            }

                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning("Unhandled exception on process packet, {@Exception}", ex);
                }
            }
        }

        private void ProcessServerInputEvent(NetworkPacket networkPacket)
        {
            var rawPeer = _rawPeerManager
                .GetPeer(peerId: networkPacket.PeerId);

            var channel = rawPeer
                .GetChannel(channelType: networkPacket.ChannelType);

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(networkPacket.HookId);

            var packetHandled = channel
                .HandleInputPacket(networkPacket: networkPacket);

            if (!packetHandled)
            {
                _logger.Debug($"FromServer NetworkPacket dropped! {networkPacket.Id}");

                return;
            }

            if (userDefinedSubscription == null)
            {
                _logger.Error($"Subscription with id {networkPacket.HookId} not found! {nameof(ProcessServerInputEvent)}");

                return;
            }

            var roomId = userDefinedSubscription.OnEvent(
                networkPacket.Serializer(),
                networkPacket.PeerId,
                _hostSettings.Serializer,
                _roomManager,
                _scheduler);

            if (networkPacket.IsReliable)
            {
                _outputQueue.Produce(new CallContext(
                    networkPacket: networkPacket,
                    resendTimeout: _hostSettings.ResendPacketsTimeout,
                    createdAt: _dateTimeProvider.UtcNow(),
                    roomId: roomId,
                    broadcastMode: BroadcastMode.AckToServer));
            }
        }

        private async Task StartReceiverAsync(
            IUdpReceiver udpReceiver)
        {
            await udpReceiver
                .StartReceiveAsync()
                .ConfigureAwait(false);
        }

        private async Task StartSenderAsync(
            IUdpSender udpSender)
        {
            foreach (var callContext in _outputQueue.Consume())
            {
                if (callContext.NetworkPacket.IsProtocolEvent)
                {
                    var protocolSubscription = _protocolSubscriptionManager
                        .GetProtocolSubscription(callContext.NetworkPacket.HookId);

                    protocolSubscription?.OnOutputEvent(
                        arg1: callContext.NetworkPacket.Serializer(),
                        arg2: callContext.NetworkPacket.PeerId);
                }

                switch (callContext.BroadcastMode)
                {
                    case BroadcastMode.Caller:
                        await _broadcastManager
                            .Caller(udpSender, callContext.NetworkPacket)
                            .ConfigureAwait(false);

                        break;

                    case BroadcastMode.Room:

                        await _broadcastManager.Room(
                            roomId: callContext.RoomId ?? throw new ArgumentNullException(nameof(callContext.RoomId)),
                            udpSender: udpSender,
                            networkPacket: callContext.NetworkPacket)
                            .ConfigureAwait(false);

                        break;

                    case BroadcastMode.RoomExceptCaller:
                        await _broadcastManager.RoomExceptCaller(
                                roomId: callContext.RoomId ?? throw new ArgumentNullException(nameof(callContext.RoomId)),
                                udpSender: udpSender,
                                networkPacket: callContext.NetworkPacket)
                            .ConfigureAwait(false);

                        break;

                    case BroadcastMode.Server:
                        await _broadcastManager.Server(
                            udpSender: udpSender,
                            networkPacket: callContext.NetworkPacket)
                            .ConfigureAwait(false);

                        break;

                    case BroadcastMode.AllPeers:
                        await _broadcastManager
                            .AllServer(udpSender, callContext.NetworkPacket)
                            .ConfigureAwait(false);

                        break;

                    case BroadcastMode.AckToServer:
                        await _broadcastManager
                            .AckToServer(udpSender, callContext.NetworkPacket)
                            .ConfigureAwait(false);

                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"Invalid broadcastMode - {callContext.BroadcastMode}");
                }
            }
        }

        private void ProcessProtocolInputEvent(
            NetworkPacket networkPacket)
        {
            var protocolHookId = networkPacket.ProtocolHookId;
            var protocolSubscription = _protocolSubscriptionManager
                .GetProtocolSubscription((byte)protocolHookId);

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            var peer = _rawPeerManager.AddOrUpdate(
                inactivityTimeout: _hostSettings.PeerInactivityTimeout,
                peerId: networkPacket.PeerId,
                ips: new List<IPEndPoint>());

            var inputHandled = peer
                .GetChannel(networkPacket.ChannelType)
                .HandleInputPacket(networkPacket);

            if (!inputHandled)
            {
                return;
            }

            switch (protocolHookId)
            {
                case ProtocolHookId.Ping:
                case ProtocolHookId.Pong:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:
                    protocolSubscription?.OnInputEvent(
                            arg1: networkPacket.Serializer(),
                            arg2: networkPacket.PeerId);

                    userDefinedSubscription?.OnProtocolEvent(
                        networkPacket.Serializer(),
                        networkPacket.PeerId,
                        _hostSettings.Serializer);
#pragma warning disable
                    // resend ack if !inputHandled
#pragma warning restore

                    break;
            }

            _outputQueue.Produce(new CallContext(
                networkPacket: networkPacket,
                resendTimeout: _hostSettings.ResendPacketsTimeout,
                createdAt: _dateTimeProvider.UtcNow(),
                roomId: null,
                broadcastMode: BroadcastMode.Caller));
        }

        private void ProcessUserDefinedInputEvent(
            NetworkPacket networkPacket)
        {
            var rawPeer = _rawPeerManager
                .GetPeer(peerId: networkPacket.PeerId);

            var channel = rawPeer
                .GetChannel(channelType: networkPacket.ChannelType);

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(networkPacket.HookId);

            var packetHandled = channel
                .HandleInputPacket(networkPacket: networkPacket);

            if (!packetHandled)
            {
                _logger.Debug($"User defined NetworkPacket dropped! {networkPacket.Id}");

                return;
            }

            if (userDefinedSubscription == null)
            {
                _logger.Error($"Subscription with id {networkPacket.HookId} not found! {nameof(ProcessUserDefinedInputEvent)}");

                return;
            }

            var roomId = userDefinedSubscription.OnEvent(
                networkPacket.Serializer(),
                networkPacket.PeerId,
                _hostSettings.Serializer,
                _roomManager,
                _scheduler);

            _outputQueue.Produce(new CallContext(
                networkPacket: networkPacket,
                resendTimeout: _hostSettings.ResendPacketsTimeout,
                createdAt: _dateTimeProvider.UtcNow(),
                roomId: roomId,
                broadcastMode: userDefinedSubscription.BroadcastMode));
        }

        private void ProcessInputAck(
            NetworkPacket networkPacket)
        {
            var rawPeer = _rawPeerManager
                .GetPeer(peerId: networkPacket.PeerId);

            var channel = rawPeer
                .GetChannel(channelType: networkPacket.ChannelType);

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(networkPacket.HookId);

            var ackHandled = channel
                .HandleAck(networkPacket: networkPacket);

            if (!ackHandled)
            {
                _logger.Debug($"Ack NetworkPacket dropped! {networkPacket.Id}");

                return;
            }

            userDefinedSubscription?
                .OnAck(networkPacket.PeerId);
        }

        private void ProcessInputProtocolAck(
            NetworkPacket networkPacket)
        {
            var protocolHookId = networkPacket.ProtocolHookId;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            var protocolSubscription = _protocolSubscriptionManager
                .GetProtocolSubscription((byte)protocolHookId);

            var peer = _rawPeerManager.AddOrUpdate(
                inactivityTimeout: _hostSettings.PeerInactivityTimeout,
                peerId: networkPacket.PeerId,
                ips: new List<IPEndPoint>());

            var ackHandled = peer
                .GetChannel(networkPacket.ChannelType)
                .HandleAck(networkPacket);

            if (!ackHandled)
            {
                _logger.Debug("Ack dropped - {@packet}", networkPacket);
                return;
            }

            switch (protocolHookId)
            {
                case ProtocolHookId.Ping:
                case ProtocolHookId.Pong:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:
                    switch (protocolHookId)
                    {
                        case ProtocolHookId.Connect:
                        case ProtocolHookId.Disconnect:
                            _serverHostClient.IsConnected = protocolHookId == ProtocolHookId.Connect;
                            break;
                    }

                    protocolSubscription?
                        .OnAck(peer.PeerId);

                    userDefinedSubscription?
                        .OnAck(networkPacket.PeerId);
                    break;
            }
        }
    }
}
