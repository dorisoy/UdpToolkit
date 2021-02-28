namespace UdpToolkit.Jobs
{
    using System.Threading.Tasks;
    using UdpToolkit.Contexts;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Queues;

    public class ClientSenderJob
    {
        private readonly IAsyncQueue<ClientOutContext> _clientOutQueue;

        public ClientSenderJob(
            IAsyncQueue<ClientOutContext> clientOutQueue)
        {
            _clientOutQueue = clientOutQueue;
        }

        public async Task ExecuteAsync(
            IUdpSender udpSender)
        {
            foreach (var outContext in _clientOutQueue.Consume())
            {
                var outPacket = outContext.OutPacket;

                await udpSender
                    .SendAsync(ref outPacket)
                    .ConfigureAwait(false);
            }
        }
    }
}