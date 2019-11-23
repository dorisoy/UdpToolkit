using System.Threading.Tasks;
using UdpToolkit.Core;
using UdpToolkit.Framework;

namespace UdpToolkit.Tests.Resources
{
    [Hub(0)]
    public class HubWithoutDependencies : HubBase
    {
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