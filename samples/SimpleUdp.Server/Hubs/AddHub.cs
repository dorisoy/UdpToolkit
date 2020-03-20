using System.Threading.Tasks;
using SimpleUdp.Contracts;
using UdpToolkit.Annotations;
using UdpToolkit.Framework.Hubs;
using UdpToolkit.Framework.Rpcs;
using UdpToolkit.Network.Clients;

namespace SimpleUdp.Server.Hubs
{
    [Hub(hubId: 0)]
    public sealed class AddHub : HubBase
    {
        private readonly IService _service;

        public AddHub(IService service)
        {
            _service = service;
        }

        [Rpc(rpcId: 0, UdpChannel.Udp)]
        public Task<SumEvent> AddUnicast(AddEvent @event)
        {
            var sum = @event.X + @event.Y;
            var response = new SumEvent
            {
                Sum = sum
            };
            
            Broadcast(response, UdpMode.Udp);

            return Task.FromResult(response);
        }
    }
}
