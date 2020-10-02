#pragma warning disable
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
        private readonly IPeer _me;
        private readonly IServerSelector _serverSelector;
        private readonly ServerHostClient _serverHostClient;

        public Host(
            int workers,
            int? pingDelayMs,
            ISerializer serializer,
            IAsyncQueue<NetworkPacket> outputQueue,
            IAsyncQueue<NetworkPacket> inputQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            ISubscriptionManager subscriptionManager,
            IPeerManager peerManager,
            IDatagramBuilder datagramBuilder,
            IRoomManager roomManager,
            IPeer me,
            IServerSelector serverSelector,
            IProtocolSubscriptionManager protocolSubscriptionManager,
            ServerHostClient serverHostClient)
        {
            _workers = workers;
            _pingDelayMs = pingDelayMs;
            _serializer = serializer;
            _outputQueue = outputQueue;
            _senders = senders;
            _receivers = receivers;
            _subscriptionManager = subscriptionManager;
            _peerManager = peerManager;
            _datagramBuilder = datagramBuilder;
            _serverHostClient = serverHostClient;
            _roomManager = roomManager;
            _me = me;
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

            // var ping = TaskUtils.RunWithRestartOnFail(
            //     job: () => StartPingHost(),
            //     logger: (exception) =>
            //     {
            //         _logger.Error("Exception on ping task: {@Exception}", exception);
            //         _logger.Warning("Restart ping tak...");
            //     },
            //     token: default);

            var workers = Enumerable
                .Range(0, _workers)
                .Select(_ => Task.Run(StartWorkerAsync))
                .ToList();

            var tasks = senders
                .Concat(receivers)
                .Concat(workers);

                // .Concat(new[] { ping });

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

        private async Task StartPingHost(
            CancellationToken token = default)
        {
            if (!_pingDelayMs.HasValue)
            {
                return;
            }

            while (!token.IsCancellationRequested)
            {
                _outputQueue.Produce(@event: new NetworkPacket(
                    channelType: ChannelType.ReliableUdp,
                    peerId: _me.PeerId,
                    channelHeader: default,
                    serializer: () => _serializer.SerializeContractLess(@event: new Ping()),
                    ipEndPoint: _serverSelector.GetServer(),
                    hookId: (byte)PacketType.Ping));

                await Task.Delay(_pingDelayMs.Value, token).ConfigureAwait(false);
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
                if (networkPacket.Type < PacketType.Ping)
                {
                    ProcessUserDefinedOutputEvent(networkPacket: networkPacket);
                }
                else
                {
                    ProcessProtocolOutputEvent(networkPacket: networkPacket);
                }

                var packet = _peerManager
                    .Get(peerId: networkPacket.PeerId)
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
            switch (networkPacket.Type)
            {
                case PacketType.P2P:
                    break;
                case PacketType.Ping:
                    _protocolSubscriptionManager.OnPing(networkPacket.PeerId, networkPacket.Serializer(), this);
                    break;
                case PacketType.Pong:
                    _protocolSubscriptionManager.OnPong(networkPacket.PeerId, networkPacket.Serializer(), this);
                    break;
                case PacketType.Ack:
                    break;
                case PacketType.Connect:
                    break;
                case PacketType.Disconnect:
                    break;
                case PacketType.Disconnected:
                    break;
                case PacketType.Connected:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
            if (networkPacket.Type < PacketType.Ping)
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

            var channel = _peerManager
                .Get(peerId: networkPacket.PeerId)
                .GetChannel(channelType: networkPacket.ChannelType);

            if (networkPacket.Type == PacketType.Ack)
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

        private void ProcessProtocolInputEvent(NetworkPacket networkPacket)
        {
            _logger.Debug("Packet received: {@packet}", networkPacket);

            var packetType = networkPacket.Type;
            switch (packetType)
            {
                case PacketType.Ping:
                    _protocolSubscriptionManager.OnPing(networkPacket.PeerId, networkPacket.Serializer(), this);

                    var ping = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (ping != null)
                    {
                        var onPing = _subscriptionManager
                            .GetSubscription(hookId: networkPacket.HookId);

                        onPing?.Invoke(
                            bytes: networkPacket.Serializer(),
                            serializer: _serializer,
                            roomManager: _roomManager,
                            datagramBuilder: _datagramBuilder,
                            peerId: networkPacket.PeerId,
                            udpMode: UdpMode.ReliableUdp);
                    }

                    break;
                case PacketType.Pong:
                    _protocolSubscriptionManager.OnPong(networkPacket.PeerId, networkPacket.Serializer(), this);

                    var pong = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (pong != null)
                    {
                        var onPong = _subscriptionManager
                            .GetSubscription(hookId: networkPacket.HookId);

                        onPong?.Invoke(
                            bytes: networkPacket.Serializer(),
                            serializer: _serializer,
                            roomManager: _roomManager,
                            datagramBuilder: _datagramBuilder,
                            peerId: networkPacket.PeerId,
                            udpMode: UdpMode.ReliableUdp);
                    }

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

                    if (connect != null)
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
                case PacketType.Connected:
                    _serverHostClient.IsConnected = true;
                    _protocolSubscriptionManager.OnConnected(networkPacket.PeerId, networkPacket.Serializer());

                    var connected = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (connected != null)
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
                case PacketType.Disconnect:
                    var disconnect = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (disconnect != null)
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
                    _serverHostClient.IsConnected = false;
                    var disconnected = _peerManager
                        .Get(networkPacket.PeerId)
                        .GetChannel(ChannelType.ReliableUdp)
                        .HandleAck(networkPacket);

                    if (disconnected != null)
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
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported packet type {packetType}!");
            }
        }
    }
}
