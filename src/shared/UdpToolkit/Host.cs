namespace UdpToolkit
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Jobs;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Pooling;
    using UdpToolkit.Network.Queues;

    public sealed class Host : IHost
    {
        private readonly IUdpToolkitLogger _udpToolkitLogger;

        private readonly HostSettings _hostSettings;

        private readonly IAsyncQueue<PooledObject<CallContext>> _outputQueue;
        private readonly IAsyncQueue<PooledObject<CallContext>> _inputQueue;

        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly ISubscriptionManager _subscriptionManager;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IObjectsPool<CallContext> _callContextPool;

        private readonly SenderJob _sendingJob;
        private readonly ReceiverJob _receivingJob;
        private readonly WorkerJob _workerJob;

        public Host(
            HostSettings hostSettings,
            IAsyncQueue<PooledObject<CallContext>> outputQueue,
            IAsyncQueue<PooledObject<CallContext>> inputQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            ISubscriptionManager subscriptionManager,
            IDateTimeProvider dateTimeProvider,
            IObjectsPool<CallContext> callContextPool,
            IScheduler scheduler,
            SenderJob sendingJob,
            ReceiverJob receivingJob,
            WorkerJob workerJob,
            IUdpToolkitLogger udpToolkitLogger)
        {
            Scheduler = scheduler;
            _hostSettings = hostSettings;
            _senders = senders;
            _receivers = receivers;
            _subscriptionManager = subscriptionManager;
            _dateTimeProvider = dateTimeProvider;
            _callContextPool = callContextPool;
            _sendingJob = sendingJob;
            _receivingJob = receivingJob;
            _workerJob = workerJob;
            _udpToolkitLogger = udpToolkitLogger;
            _inputQueue = inputQueue;
            _outputQueue = outputQueue;
        }

        public IHostClient HostClient => _workerJob.HostClient;

        public IScheduler Scheduler { get; }

        public async Task RunAsync()
        {
            var senders = _senders
                .Select(
                    sender => TaskUtils.RestartOnFail(
                        job: () => _sendingJob.Execute(sender),
                        logger: (exception) =>
                        {
                            _udpToolkitLogger.Error($"Exception on send task: {exception}");
                            _udpToolkitLogger.Warning("Restart sender...");
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

            var workers = Enumerable
                .Range(0, _hostSettings.Workers)
                .Select(_ => Task.Run(_workerJob.Execute))
                .ToList();

            var tasks = senders
                .Concat(receivers)
                .Concat(workers);

            _udpToolkitLogger.Information($"{nameof(Host)} running...");

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);
        }

        public void Stop()
        {
            _inputQueue.Stop();
            _outputQueue.Stop();
        }

        public void OnCore(
            byte hookId,
            Subscription subscription)
        {
            _subscriptionManager.Subscribe(hookId, subscription);
        }

        public void SendCore<TEvent>(
            TEvent @event,
            int roomId,
            byte hookId,
            UdpMode udpMode,
            BroadcastMode broadcastMode,
            IPEndPoint ipEndPoint = null)
        {
            var pooledCallContext = _callContextPool.Get();

            pooledCallContext.Value.Set(
                resendTimeout: _hostSettings.ResendPacketsTimeout,
                createdAt: _dateTimeProvider.UtcNow(),
                roomId: roomId,
                broadcastMode: broadcastMode);

            pooledCallContext.Value.NetworkPacketDto.Set(
                hookId: hookId,
                channelType: udpMode.Map(),
                networkPacketType: NetworkPacketType.FromServer,
                peerId: default,
                id: default,
                acks: default,
                serializer: () => _hostSettings.Serializer.Serialize(@event),
                createdAt: _dateTimeProvider.UtcNow(),
                ipEndPoint: ipEndPoint);

            _outputQueue.Produce(pooledCallContext);
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
    }
}
