namespace UdpToolkit.Jobs
{
    using System.Threading.Tasks;
    using UdpToolkit.Contexts;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Queues;

    public sealed class HostSenderJob
    {
        private readonly IAsyncQueue<HostOutContext> _hostOutQueue;

        public HostSenderJob(
            IAsyncQueue<HostOutContext> hostOutQueue)
        {
            _hostOutQueue = hostOutQueue;
        }

        public async Task Execute(
            IUdpSender udpSender)
        {
            foreach (var outContext in _hostOutQueue.Consume())
            {
                var outPacket = outContext.OutPacket;

                await udpSender
                    .SendAsync(ref outPacket)
                    .ConfigureAwait(false);
            }
        }
    }
}
