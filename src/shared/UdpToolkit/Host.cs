namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UdpToolkit.Contexts;
    using UdpToolkit.Core;
    using UdpToolkit.Jobs;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Queues;

    public sealed class Host : IHost
    {
        private readonly IUdpToolkitLogger _udpToolkitLogger;

        private readonly HostSettings _hostSettings;

        private readonly IAsyncQueue<InContext> _inputQueue;

        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly ISubscriptionManager _subscriptionManager;

        private readonly HostSenderJob _hostSendingJob;
        private readonly ReceiverJob _receivingJob;
        private readonly WorkerJob _workerJob;
        private readonly ClientSenderJob _clientSenderJob;
        private readonly IBroadcaster _broadcaster;

        public Host(
            HostSettings hostSettings,
            IAsyncQueue<InContext> inputQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            ISubscriptionManager subscriptionManager,
            IScheduler scheduler,
            HostSenderJob hostSendingJob,
            ReceiverJob receivingJob,
            WorkerJob workerJob,
            ClientSenderJob clientSenderJob,
            IUdpToolkitLogger udpToolkitLogger,
            IBroadcaster broadcaster)
        {
            Scheduler = scheduler;
            _hostSettings = hostSettings;
            _senders = senders;
            _receivers = receivers;
            _subscriptionManager = subscriptionManager;
            _hostSendingJob = hostSendingJob;
            _receivingJob = receivingJob;
            _workerJob = workerJob;
            _udpToolkitLogger = udpToolkitLogger;
            _broadcaster = broadcaster;
            _clientSenderJob = clientSenderJob;
            _inputQueue = inputQueue;
        }

        public IHostClient HostClient => _workerJob.HostClient;

        public IScheduler Scheduler { get; }

        public async Task RunAsync()
        {
            var hostSenders = _senders
                .Select(
                    sender => TaskUtils.RestartOnFail(
                        job: () => _hostSendingJob.Execute(sender),
                        logger: (exception) =>
                        {
                            _udpToolkitLogger.Error($"Exception on send task: {exception}");
                            _udpToolkitLogger.Warning("Restart host sender...");
                        },
                        token: default))
                .ToList();

            var receivers = _receivers
                .Select(
                    receiver => TaskUtils.RestartOnFail(
                        job: () => _receivingJob.Execute(receiver),
                        logger: (exception) =>
                        {
                            _udpToolkitLogger.Error($"Exception on receive task: {exception}");
                            _udpToolkitLogger.Warning("Restart receiver...");
                        },
                        token: default))
                .ToList();

            var clientSenders = _senders
                .Select(
                    sender => TaskUtils.RestartOnFail(
                        job: () => _clientSenderJob.ExecuteAsync(sender),
                        logger: (exception) =>
                        {
                            _udpToolkitLogger.Error($"Exception on receive task: {exception}");
                            _udpToolkitLogger.Warning("Restart client sender...");
                        },
                        token: default))
                .ToList();

            var workers = Enumerable
                .Range(0, _hostSettings.Workers)
                .Select(_ => Task.Run(_workerJob.Execute))
                .ToList();

            var tasks = hostSenders
                .Concat(receivers)
                .Concat(workers)
                .Concat(clientSenders);

            _udpToolkitLogger.Information($"{nameof(Host)} running...");

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);
        }

        public void Stop()
        {
            _inputQueue.Stop();
        }

        public void OnCore(
            byte hookId,
            Subscription subscription)
        {
            _subscriptionManager.Subscribe(hookId, subscription);
        }

        public void SendCore<TEvent>(
            TEvent @event,
            Guid caller,
            int roomId,
            byte hookId,
            UdpMode udpMode,
            BroadcastMode broadcastMode)
        {
            _broadcaster.Broadcast(
                serializer: () => _hostSettings.Serializer.Serialize(@event),
                packetType: PacketType.FromServer,
                caller: caller,
                roomId: roomId,
                hookId: hookId,
                channelType: udpMode.Map(),
                broadcastMode: broadcastMode);
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
            _broadcaster.Dispose();
        }
    }
}
