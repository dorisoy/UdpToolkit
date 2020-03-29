namespace UdpToolkit.Framework.Hosts.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class ServerHost : IServerHost
    {
        private readonly ILogger _logger = Log.ForContext<ServerHost>();

        private readonly IAsyncQueue<NetworkPacket> _inputQueue;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;

        private readonly int _processWorkers;
        private readonly IReadOnlyCollection<IUdpSender> _senders;
        private readonly IReadOnlyCollection<IUdpReceiver> _receivers;

        private readonly IPipeline _pipeline;

        public ServerHost(
            int processWorkers,
            IAsyncQueue<NetworkPacket> outputQueue,
            IAsyncQueue<NetworkPacket> inputQueue,
            IReadOnlyCollection<IUdpSender> senders,
            IReadOnlyCollection<IUdpReceiver> receivers,
            IPipeline pipeline)
        {
            _processWorkers = processWorkers;
            _outputQueue = outputQueue;
            _inputQueue = inputQueue;
            _senders = senders;
            _receivers = receivers;
            _pipeline = pipeline;

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

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);
        }

        private async Task ProcessPacketAsync(NetworkPacket networkPacket)
        {
            await _pipeline
                .ExecuteAsync(new CallContext(
                    hubId: networkPacket.FrameworkHeader.HubId,
                    rpcId: networkPacket.FrameworkHeader.RpcId,
                    scopeId: networkPacket.FrameworkHeader.ScopeId,
                    udpMode: networkPacket.UdpMode.Map(),
                    payload: networkPacket.Payload,
                    peerIPs: networkPacket.Peers.Select(x => x.IpEndPoint)))
                .ConfigureAwait(false);
        }

        private async Task StartWorkerAsync()
        {
            foreach (var networkPacket in _inputQueue.Consume())
            {
                try
                {
                    await ProcessPacketAsync(networkPacket: networkPacket)
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
            foreach (var networkPacket in _outputQueue.Consume())
            {
                await udpSender
                    .SendAsync(networkPacket)
                    .ConfigureAwait(false);
            }
        }
    }
}
