namespace UdpToolkit.Tests.Resources
{
    using System.Threading.Tasks;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Server.Hubs;
    using UdpToolkit.Framework.Server.Rpcs;
    using Xunit;

    [Hub(1)]
    public sealed class HubWithDependencies : HubBase
    {
        private readonly TestService _service;

        public HubWithDependencies(TestService service)
        {
            _service = service;
            Assert.NotNull(service);
        }

        [Rpc(0, UdpChannel.Udp)]
        public async Task FuncWithoutArgs()
        {
            await Task.CompletedTask
                .ConfigureAwait(false);

            await Clients
                .All()
                .SendAsync(
                    @event: new { },
                    hubContext: HubContext)
                .ConfigureAwait(false);
        }

        [Rpc(1, UdpChannel.Udp)]
        public async Task FuncWithArgs(Message message)
        {
            await Task.CompletedTask
                .ConfigureAwait(false);

            await Clients
                .All()
                .SendAsync(
                    @event: new { },
                    hubContext: HubContext)
                .ConfigureAwait(false);
        }

        public Task Foo(Message message)
        {
            throw new System.NotImplementedException();
        }

        [Rpc(2, UdpChannel.Udp)]
        public async Task FuncWithArgs(int x, int y)
        {
            await Task.CompletedTask
                .ConfigureAwait(false);

            await Clients
                .All()
                .SendAsync(
                    @event: new { },
                    hubContext: HubContext)
                .ConfigureAwait(false);
        }
    }
}