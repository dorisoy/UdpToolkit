namespace UdpToolkit.Jobs
{
    using System.Threading.Tasks;
    using UdpToolkit.Contexts;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Queues;

    public sealed class ReceiverJob
    {
        private readonly HostSettings _hostSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAsyncQueue<InContext> _inputQueue;

        public ReceiverJob(
            HostSettings hostSettings,
            IDateTimeProvider dateTimeProvider,
            IAsyncQueue<InContext> inputQueue)
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

                var inPacket = valueTuple.Item1;
                var success = valueTuple.Item2;

                if (!success)
                {
                    continue;
                }

                // TODO Select queue by hashed connectionId
                _inputQueue.Produce(
                    new InContext(
                        resendTimeout: _hostSettings.ResendPacketsTimeout,
                        createdAt: _dateTimeProvider.UtcNow(),
                        inPacket: inPacket));
            }
        }
    }
}