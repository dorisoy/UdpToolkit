namespace UdpToolkit.Jobs
{
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Queues;

    public sealed class ReceiverJob
    {
        private readonly HostSettings _hostSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAsyncQueue<CallContext> _inputQueue;

        public ReceiverJob(
            HostSettings hostSettings,
            IDateTimeProvider dateTimeProvider,
            IAsyncQueue<CallContext> inputQueue)
        {
            _hostSettings = hostSettings;
            _dateTimeProvider = dateTimeProvider;
            _inputQueue = inputQueue;
        }

        public async Task Execute(
            IUdpReceiver udpReceiver)
        {
            while (true)
            {
                var valueTuple = await udpReceiver
                    .ReceiveAsync()
                    .ConfigureAwait(false);

                var networkPacket = valueTuple.Item1;
                var success = valueTuple.Item2;

                if (!success)
                {
                    continue;
                }

                _inputQueue.Produce(
                    new CallContext(
                        resendTimeout: _hostSettings.ResendPacketsTimeout,
                        createdAt: _dateTimeProvider.UtcNow(),
                        roomId: null,
                        broadcastMode: null,
                        networkPacket: networkPacket));
            }
        }
    }
}