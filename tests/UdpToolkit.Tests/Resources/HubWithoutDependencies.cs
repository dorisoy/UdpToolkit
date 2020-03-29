namespace UdpToolkit.Tests.Resources
{
    using System.Threading.Tasks;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Hubs;
    using UdpToolkit.Framework.Rpcs;

    [Hub(0)]
    public sealed class HubWithoutDependencies : HubBase
    {
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

        [Rpc(2, UdpChannel.Udp)]
        public async Task<IRpcResult> FuncWithArgs(int x, int y)
        {
            await Task.CompletedTask
                .ConfigureAwait(false);

            return Broadcast(new { });
        }
    }
}