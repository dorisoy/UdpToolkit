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
        public async Task Join(JoinEvent @event)
        {
            Rooms.JoinOrCreate(@event.RoomId, HubContext.PeerId);

            await Clients
                .Room(@event.RoomId)
                .SendAsync(
                    @event: new JoinedEvent
                    {
                        Nickname = @event.Nickname,
                    },
                    hubContext: HubContext)
                .ConfigureAwait(false);
        }

        [Rpc(rpcId: 1, UdpChannel.Udp)]
        public async Task Leave(LeaveEvent @event)
        {
            Rooms.Leave(roomId: @event.RoomId, peerId: HubContext.PeerId);

            await Clients
                .Room(@event.RoomId)
                .SendAsync(
                    @event: new LeavedEvent
                    {
                        Nickname = @event.Nickname,
                    },
                    hubContext: HubContext)
                .ConfigureAwait(false);
        }

        [Rpc(rpcId: 2, UdpChannel.Udp)]
        public async Task Move(MoveEvent @event)
        {
            await Clients
                .Room(@event.RoomId)
                .SendAsync(
                    @event: @event,
                    hubContext: HubContext)
                .ConfigureAwait(false);
        }
    }
}
