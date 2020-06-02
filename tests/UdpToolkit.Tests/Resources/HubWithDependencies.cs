namespace UdpToolkit.Tests.Resources
{
    using System.Threading.Tasks;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Server.Core;
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
        public async Task<IRpcResult> FuncWithoutArgs()
        {
            await Task.CompletedTask
                .ConfigureAwait(false);

            return Broadcast(new { });
        }

        [Rpc(1, UdpChannel.Udp)]
        public async Task<IRpcResult> FuncWithArgs(Message message)
        {
            await Task.CompletedTask
                .ConfigureAwait(false);

            return Broadcast(new { });
        }

        public Task Foo(Message message)
        {
            throw new System.NotImplementedException();
        }

        [Rpc(2, UdpChannel.Udp)]
        public async Task<IRpcResult> FuncWithArgs(int x, int y)
        {
            await Task.CompletedTask
                .ConfigureAwait(false);

            return Broadcast(new { });
        }
    }
}