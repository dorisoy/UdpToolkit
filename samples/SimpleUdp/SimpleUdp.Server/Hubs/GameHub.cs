namespace SimpleUdp.Server.Hubs
{
    using System.Threading.Tasks;
    using SimpleUdp.Contracts;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Server.Hubs;
    using UdpToolkit.Framework.Server.Rpcs;

    [Hub(hubId: 0)]
    public sealed class GameHub : HubBase
    {
        [Rpc(rpcId: 0, UdpChannel.Udp)]
        public async Task Test(JoinEvent joinEvent)
        {
            Rooms.JoinOrCreate(joinEvent.RoomId, HubContext.PeerId);

            await Clients
                .Room(joinEvent.RoomId)
                .SendAsync(
                    @event: joinEvent,
                    hubContext: HubContext)
                .ConfigureAwait(false);
        }
    }
}
