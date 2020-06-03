namespace UdpToolkit.Tests.Resources
{
    using System.Threading.Tasks;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Server.Hubs;
    using UdpToolkit.Framework.Server.Rpcs;

    [Hub(0)]
    public sealed class HubWithoutDependencies : HubBase
    {
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