namespace UdpToolkit.Jobs
{
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Pooling;
    using UdpToolkit.Network.Queues;

    public sealed class ReceiverJob
    {
        private readonly HostSettings _hostSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IObjectsPool<CallContext> _callContextPool;
        private readonly IAsyncQueue<PooledObject<CallContext>> _inputQueue;

        public ReceiverJob(
            HostSettings hostSettings,
            IDateTimeProvider dateTimeProvider,
            IObjectsPool<CallContext> callContextPool,
            IAsyncQueue<PooledObject<CallContext>> inputQueue)
        {
            _hostSettings = hostSettings;
            _dateTimeProvider = dateTimeProvider;
            _callContextPool = callContextPool;
            _inputQueue = inputQueue;
        }

        public async Task Execute(
            IUdpReceiver udpReceiver)
        {
            while (true)
            {
                using var pooledNetworkPacket = await udpReceiver
                    .ReceiveAsync()
                    .ConfigureAwait(false);

                if (pooledNetworkPacket == null)
                {
                    // if null - packet dropped
                    continue;
                }

                var networkPacket = pooledNetworkPacket.Value;
                var pooledCallContext = _callContextPool.Get();

                pooledCallContext.Value.Set(
                    resendTimeout: _hostSettings.ResendPacketsTimeout,
                    createdAt: _dateTimeProvider.UtcNow(),
                    roomId: null,
                    broadcastMode: null);

                pooledCallContext.Value.NetworkPacketDto.Set(
                    id: networkPacket.Id,
                    acks: networkPacket.Acks,
                    hookId: networkPacket.HookId,
                    channelType: networkPacket.ChannelType,
                    peerId: networkPacket.PeerId,
                    networkPacketType: networkPacket.NetworkPacketType,
                    serializer: networkPacket.Serializer,
                    createdAt: networkPacket.CreatedAt,
                    ipEndPoint: networkPacket.IpEndPoint);

                _inputQueue.Produce(pooledCallContext);
            }
        }
    }
}