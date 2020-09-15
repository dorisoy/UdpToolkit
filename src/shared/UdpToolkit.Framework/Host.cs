namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Serialization;

    public sealed class Host : IHost
    {
        private readonly ILogger _logger = Log.ForContext<Host>();

        private readonly int _workers;
        private readonly ISerializer _serializer;

        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly IAsyncQueue<NetworkPacket> _inputQueue;

        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly IPeerManager _peerManager;
        private readonly IDatagramBuilder _datagramBuilder;

        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IProtocolSubscriptionManager _protocolSubscriptionManager;

        private readonly IRoomManager _roomManager;

        public Host(
            int workers,
            ISerializer serializer,
            IAsyncQueue<NetworkPacket> outputQueue,
            IAsyncQueue<NetworkPacket> inputQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            ISubscriptionManager subscriptionManager,
            IPeerManager peerManager,
            IDatagramBuilder datagramBuilder,
            IServerHostClient serverHostClient,
            IRoomManager roomManager,
            IProtocolSubscriptionManager protocolSubscriptionManager)
        {
            _workers = workers;
            _serializer = serializer;
            _outputQueue = outputQueue;
            _senders = senders;
            _receivers = receivers;
            _subscriptionManager = subscriptionManager;
            _peerManager = peerManager;
            _datagramBuilder = datagramBuilder;
            ServerHostClient = serverHostClient;
            _roomManager = roomManager;
            _protocolSubscriptionManager = protocolSubscriptionManager;
            _inputQueue = inputQueue;

            foreach (var receiver in _receivers)
            {
                receiver.UdpPacketReceived += OnUdpPacketReceived;
            }
        }

        public IServerHostClient ServerHostClient { get; }

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

            var workers = Enumerable
                .Range(0, _workers)
                .Select(_ => Task.Run(StartWorkerAsync))
                .ToList();

            var tasks = senders.Concat(receivers).Concat(workers);

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

        public void PublishCore<TEvent>(Func<IDatagramBuilder, Datagram<TEvent>> datagramFactory, UdpMode udpMode)
        {
            var datagram = datagramFactory(_datagramBuilder);

            foreach (var peer in datagram.Peers)
            {
                _outputQueue.Produce(
                    @event: new NetworkPacket(
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
                var channel = _peerManager
                    .Get(peerId: networkPacket.PeerId)
                    .GetChannel(networkPacket.ChannelType);

                var packet = channel.TryHandleOutputPacket(networkPacket: networkPacket);
                if (packet.HasValue)
                {
                    await udpSender
                        .SendAsync(packet.Value)
                        .ConfigureAwait(false);
                }
            }
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
            if (networkPacket.HookId < (byte)PacketType.Ping)
            {
                ProcessUserDefinedEvent(networkPacket: networkPacket);
            }
            else
            {
                ProcessProtocolEvent(networkPacket: networkPacket);
            }
        }

        private void ProcessUserDefinedEvent(NetworkPacket networkPacket)
        {
            _logger.Debug("Packet received: {@packet}", networkPacket);

            var channel = _peerManager
                .Get(peerId: networkPacket.PeerId)
                .GetChannel(channelType: networkPacket.ChannelType);

            if (networkPacket.HookId == (byte)PacketType.Ack)
            {
                var result = channel.HandleAck(networkPacket: networkPacket);

                if (result.HasValue)
                {
                    _inputQueue.Produce(result.Value);
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

        private void ProcessProtocolEvent(NetworkPacket networkPacket)
        {
            _logger.Debug("Packet received: {@packet}", networkPacket);

            var packetType = (PacketType)networkPacket.HookId;
            switch (packetType)
            {
                case PacketType.Ping:
                    break;
                case PacketType.Pong:
                    break;
                case PacketType.Ack:
                    break;
                case PacketType.P2P:
                    break;
                case PacketType.Connect:
                    _protocolSubscriptionManager.OnConnect(networkPacket.PeerId, networkPacket.Serializer(), this);

                    var connect = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (connect.HasValue)
                    {
                        var onConnect = _subscriptionManager
                            .GetSubscription(hookId: networkPacket.HookId);

                        onConnect?.Invoke(
                            bytes: networkPacket.Serializer(),
                            serializer: _serializer,
                            roomManager: _roomManager,
                            datagramBuilder: _datagramBuilder,
                            peerId: networkPacket.PeerId,
                            udpMode: UdpMode.ReliableUdp);
                    }

                    break;
                case PacketType.Disconnect:
                    var disconnect = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (disconnect.HasValue)
                    {
                        var onDisconnect = _subscriptionManager
                            .GetSubscription(networkPacket.HookId);

                        onDisconnect?.Invoke(
                            bytes: networkPacket.Serializer(),
                            peerId: networkPacket.PeerId,
                            serializer: _serializer,
                            roomManager: _roomManager,
                            datagramBuilder: _datagramBuilder,
                            udpMode: UdpMode.ReliableUdp);

                        _protocolSubscriptionManager.OnDisconnect(networkPacket.PeerId, networkPacket.Serializer(), this);
                    }

                    break;
                case PacketType.Disconnected:
                    var disconnected = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (disconnected.HasValue)
                    {
                        var onDisconnected = _subscriptionManager
                            .GetSubscription(networkPacket.HookId);

                        onDisconnected?.Invoke(
                            serializer: _serializer,
                            roomManager: _roomManager,
                            datagramBuilder: _datagramBuilder,
                            bytes: networkPacket.Serializer(),
                            peerId: networkPacket.PeerId,
                            udpMode: UdpMode.ReliableUdp);

                        _protocolSubscriptionManager.OnDisconnected(networkPacket.PeerId, networkPacket.Serializer());
                    }

                    break;
                case PacketType.Connected:
                    _protocolSubscriptionManager.OnConnected(networkPacket.PeerId, networkPacket.Serializer());

                    var connected = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (connected.HasValue)
                    {
                        var onConnected = _subscriptionManager
                            .GetSubscription(networkPacket.HookId);

                        onConnected?.Invoke(
                            serializer: _serializer,
                            roomManager: _roomManager,
                            datagramBuilder: _datagramBuilder,
                            bytes: networkPacket.Serializer(),
                            peerId: networkPacket.PeerId,
                            udpMode: UdpMode.ReliableUdp);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported packet type {packetType}!");
            }
        }
    }
}
