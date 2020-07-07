namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Serialization;

    public sealed class ServerHostClient : IServerHostClient
    {
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly IServerSelector _serverSelector;
        private readonly ISerializer _serializer;
        private readonly List<IPEndPoint> _peerIps;
        private readonly IPeerManager _peerManager;
        private readonly ISubscriptionManager _subscriptionManager;

        private readonly Guid _me;

        public ServerHostClient(
            IAsyncQueue<NetworkPacket> outputQueue,
            IServerSelector serverSelector,
            ISerializer serializer,
            List<IPEndPoint> peerIps,
            IPeerManager peerManager,
            ISubscriptionManager subscriptionManager)
        {
            _outputQueue = outputQueue;
            _serverSelector = serverSelector;
            _serializer = serializer;
            _peerIps = peerIps;
            _peerManager = peerManager;
            _subscriptionManager = subscriptionManager;

            _me = Guid.NewGuid();
        }

        public void Connect(TimeSpan timeout)
        {
            _peerManager.Create(_me, _peerIps);

            var inputPorts = _peerIps
                .Select(x => x.Port)
                .ToList();

            var host = _peerIps.First().Address.ToString();
            var @event = new Connect(host, inputPorts);
            var serverIp = _serverSelector.GetServer();

            var manualResetEvent = new ManualResetEvent(initialState: false);
            PublishInternal(@event: @event, ipEndPoint: serverIp, hookId: (byte)PacketType.Connect, udpMode: UdpMode.ReliableUdp, serializer: _serializer.SerializeContractLess);

            _subscriptionManager.OnProtocolInternal<Connected>(
                handler: (peerId, connected) =>
                {
                    Log.Logger.Information($"Connected with id - {peerId}");
                    manualResetEvent.Set();
                },
                hookId: (byte)PacketType.Connect);

            manualResetEvent.WaitOne(timeout);
        }

        public void Disconnect(TimeSpan timeout)
        {
            var serverIp = _serverSelector.GetServer();
            var @event = new Disconnect();

            var manualResetEvent = new ManualResetEvent(initialState: false);
            PublishInternal(@event: @event, ipEndPoint: serverIp, hookId: (byte)PacketType.Disconnect, udpMode: UdpMode.ReliableUdp, serializer: _serializer.SerializeContractLess);

            _subscriptionManager.OnProtocolInternal<Disconnected>(
                handler: (peerId, disconnected) =>
                {
                    Log.Logger.Information($"Peer with id - {peerId} disconnected");
                    manualResetEvent.Set();
                },
                hookId: (byte)PacketType.Disconnect);

            manualResetEvent.WaitOne(timeout);
        }

        public void Publish<TEvent>(TEvent @event, byte hookId, UdpMode udpMode)
        {
            var serverIp = _serverSelector.GetServer();

            PublishInternal(@event: @event, ipEndPoint: serverIp, hookId: hookId, udpMode: udpMode, _serializer.Serialize);
        }

        public void PublishP2P<TEvent>(TEvent @event, IPEndPoint ipEndPoint, byte hookId, UdpMode udpMode)
        {
            PublishInternal(@event: @event, ipEndPoint: ipEndPoint, hookId: hookId, udpMode: udpMode, _serializer.Serialize);
        }

        private void PublishInternal<TEvent>(TEvent @event, IPEndPoint ipEndPoint, byte hookId, UdpMode udpMode, Func<TEvent, byte[]> serializer)
        {
            if (!_peerManager.Exist(_me))
            {
                _peerManager.Create(peerId: _me, peerIps: _peerIps);
            }

            _outputQueue.Produce(new NetworkPacket(
                channelType: udpMode.Map(),
                peerId: _me,
                channelHeader: default,
                serializer: () => serializer(@event),
                ipEndPoint: ipEndPoint,
                hookId: hookId));
        }
    }
}
