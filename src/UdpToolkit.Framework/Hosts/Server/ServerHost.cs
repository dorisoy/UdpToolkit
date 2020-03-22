namespace UdpToolkit.Framework.Hosts.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Hubs;
    using UdpToolkit.Framework.Rpcs;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Rudp;

    public sealed class ServerHost : IServerHost
    {
        private readonly ILogger _logger = Log.ForContext<ServerHost>();

        private readonly IAsyncQueue<InputUdpPacket> _inputQueue;
        private readonly IAsyncQueue<OutputUdpPacket> _outputQueue;

        private readonly int _processWorkers;
        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly IPeerTracker _peerTracker;
        private readonly IContainer _container;
        private readonly IRpcProvider _rpcProvider;
        private readonly ISerializer _serializer;

        public ServerHost(
            int processWorkers,
            IAsyncQueue<OutputUdpPacket> outputQueue,
            IAsyncQueue<InputUdpPacket> inputQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            IPeerTracker peerTracker,
            IContainer container,
            IRpcProvider rpcProvider,
            ISerializer serializer)
        {
            _processWorkers = processWorkers;
            _outputQueue = outputQueue;
            _rpcProvider = rpcProvider;
            _serializer = serializer;
            _inputQueue = inputQueue;
            _senders = senders;
            _receivers = receivers;
            _peerTracker = peerTracker;
            _container = container;

            foreach (var receiver in _receivers)
            {
                receiver.UdpPacketReceived += packet =>
                {
                    _logger.Debug("Packet received: {@packet}", packet);

                    _inputQueue.Produce(packet);
                };
            }
        }

        public async Task RunAsync()
        {
            var senders = _senders
                .Select(
                    sender => Task.Run(
                        () => StartSenderAsync(sender)
                            .RestartJobOnFailAsync(
                                job: () => StartSenderAsync(sender),
                                logger: (exception) =>
                                {
                                    _logger.Error("Exception on send task: {@Exception}", exception);
                                    _logger.Warning("Restart sender...");
                                })))
                .ToList();

            var receivers = _receivers
                .Select(
                    receiver => Task.Run(
                        () => StartReceiverAsync(receiver)
                            .RestartJobOnFailAsync(
                                job: () => StartReceiverAsync(receiver),
                                logger: (exception) =>
                                {
                                    _logger.Error("Exception on receive task: {@Exception}", exception);
                                    _logger.Warning("Restart receiver...");
                                })))
                .ToList();

            var workers = Enumerable.Range(0, _processWorkers)
                .Select(
                    _ => Task.Run(
                        () => StartWorkerAsync()
                            .RestartJobOnFailAsync(
                                job: StartWorkerAsync,
                                logger: (exception) =>
                                {
                                    _logger.Error("Exception on worker task: {@Exception}", exception);
                                    _logger.Warning("Restart worker...");
                                })))
                    .ToList();

            var tasks = senders.Concat(receivers).Concat(workers);

            _logger.Information($"{nameof(ServerHost)} running...");

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);
        }

        private async Task ProcessPacketAsync(InputUdpPacket inputUdpPacket)
        {
            var getResult = _peerTracker.TryGetPeer(scopeId: inputUdpPacket.ScopeId, peerId: inputUdpPacket.PeerId, out var peer);
            if (!getResult)
            {
                _peerTracker.AddPeer(
                    scopeId: inputUdpPacket.ScopeId,
                    peer: new Peer(
                        id: inputUdpPacket.PeerId,
                        remotePeer: inputUdpPacket.RemotePeer,
                        reliableUdpChannel: new ReliableUdpChannel()));
            }

            var key = new RpcDescriptorId(hubId: inputUdpPacket.HubId, rpcId: inputUdpPacket.RpcId);
            if (!_rpcProvider.TryProvide(key, out var rpcDescriptor))
            {
                _logger.Warning("Rpc not found by rpcDescriptor: {@rpcDescriptor}", rpcDescriptor);

                return;
            }

            if (rpcDescriptor.ParametersTypes.Count > 1)
            {
                _logger.Warning("Rpc not support more than one argument");

                return;
            }

            // TODO check dependencies
            // TODO check method args
            var @event = rpcDescriptor.ParametersTypes
                .Select(type => _serializer.Deserialize(type, inputUdpPacket.Payload))
                .ToArray();

            await rpcDescriptor
                    .HubRpc(
                        hubContext: new HubContext(
                            scopeId: inputUdpPacket.ScopeId,
                            hubId: inputUdpPacket.HubId,
                            rpcId: inputUdpPacket.RpcId,
                            peerId: inputUdpPacket.PeerId),
                        serializer: _serializer,
                        peerTracker: _peerTracker,
                        eventProducer: _outputQueue,
                        ctorArguments: _container
                            .GetInstances(rpcDescriptor.CtorArguments)
                            .ToArray(),
                        methodArguments: @event)
                    .ConfigureAwait(false);
        }

        private async Task StartWorkerAsync()
        {
            foreach (var inputUdpPacket in _inputQueue.Consume())
            {
                try
                {
                    await ProcessPacketAsync(inputUdpPacket: inputUdpPacket)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Unhandled exception on process packet, {@Exception}", ex);
                }
            }
        }

        private async Task StartReceiverAsync(IUdpReceiver udpReceiver)
        {
            await udpReceiver.StartReceiveAsync()
                .ConfigureAwait(false);
        }

        private async Task StartSenderAsync(IUdpSender udpSender)
        {
            foreach (var outputUdpPacket in _outputQueue.Consume())
            {
                await udpSender.SendAsync(outputUdpPacket)
                    .ConfigureAwait(false);
            }
        }
    }
}
