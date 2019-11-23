using System.Threading.Tasks;
using UdpToolkit.Core;
using SimpleUdp.Contracts;
using UdpToolkit.Framework;

namespace SimpleUdp.Server.Hubs
{
    [Hub(0)]
    public sealed class AddHub : HubBase
    {
        private readonly IService _service;

        public AddHub(IService service)
        {
            _service = service;
        }
        
        [Rpc(0)]
        public Task AddUnicast(AddRequest request)
        {
            var sum = request.X + request.Y;
            var response = new AddResponse
            {
                Sum = sum
            };
            
            Unicast(response, UdpMode.Udp);

            return Task.CompletedTask;
        }
        
        [Rpc(1)]
        public Task AddMultiCast(AddRequest request)
        {
            var sum = request.X + request.Y;
            var response = new AddResponse
            {
                Sum = sum
            };
            
            Broadcast(response, UdpMode.Udp);
            
            return Task.CompletedTask;
        }
    }
}
