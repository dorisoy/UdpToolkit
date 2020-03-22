namespace UdpToolkit.Tests.Resources
{
    using System.Threading.Tasks;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Hubs;
    using UdpToolkit.Framework.Rpcs;
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
        public Task FuncWithoutArgs()
        {
            this.AssertAllPropertiesInitialized();

            return Task.CompletedTask;
        }

        [Rpc(1, UdpChannel.Udp)]
        public Task FuncWithArgs(Message message)
        {
            this.AssertAllPropertiesInitialized();

            return Task.CompletedTask;
        }

        public Task Foo(Message message)
        {
            throw new System.NotImplementedException();
        }

        [Rpc(2, UdpChannel.Udp)]
        public Task FuncWithArgs(int x, int y)
        {
            this.AssertAllPropertiesInitialized();

            return Task.CompletedTask;
        }
    }
}