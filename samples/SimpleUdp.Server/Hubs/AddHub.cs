namespace SimpleUdp.Server.Hubs
{
    using System.Threading.Tasks;
    using SimpleUdp.Contracts;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Hubs;
    using UdpToolkit.Framework.Rpcs;

    [Hub(hubId: 0)]
    public sealed class AddHub : HubBase
    {
        private readonly IService _service;

        public AddHub(IService service)
        {
            _service = service;
        }

        [Rpc(rpcId: 0, UdpChannel.Udp)]
        public async Task<IRpcResult> AddBroadcast(AddEvent @event)
        {
            var sum = @event.X + @event.Y;
            var response = new SumEvent
            {
                Sum = sum,
            };

            await _service
                .ProcessAsync()
                .ConfigureAwait(false);

            return Broadcast(response);
        }
    }
}
