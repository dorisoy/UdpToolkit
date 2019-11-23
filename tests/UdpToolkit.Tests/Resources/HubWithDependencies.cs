using System.Threading.Tasks;
using UdpToolkit.Core;
using UdpToolkit.Framework;
using Xunit;

namespace UdpToolkit.Tests.Resources
{
    [Hub(1)]
    public class HubWithDependencies : HubBase
    {
        private readonly TestService _service;
        
        public HubWithDependencies(TestService service)
        {
            _service = service;
            Assert.NotNull(service);
        }
        
        [Rpc(0)]
        public Task FuncWithoutArgs()
        {
            this.AssertAllPropertiesInitialized();
            
            return Task.CompletedTask;
        }
        
        [Rpc(1)]
        public Task FuncWithArgs(Message message)
        {
            this.AssertAllPropertiesInitialized();
            
            return Task.CompletedTask;
        }
        
        [Rpc(2)]
        public Task FuncWithArgs(int x, int y)
        {
            this.AssertAllPropertiesInitialized();
            
            return Task.CompletedTask;
        }
    }
}