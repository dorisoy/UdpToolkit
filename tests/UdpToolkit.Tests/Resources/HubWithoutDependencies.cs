using System.Threading.Tasks;
using UdpToolkit.Annotations;
using UdpToolkit.Framework;
using UdpToolkit.Framework.Hubs;
using UdpToolkit.Framework.Rpcs;

namespace UdpToolkit.Tests.Resources
{
    [Hub(0)]
    public sealed class HubWithoutDependencies : HubBase
    {
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

        [Rpc(2, UdpChannel.Udp)]
        public Task FuncWithArgs(int x, int y)
        {
            this.AssertAllPropertiesInitialized();
            
            return Task.CompletedTask;
        }
    }
}