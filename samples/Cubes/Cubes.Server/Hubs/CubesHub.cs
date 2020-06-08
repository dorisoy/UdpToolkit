namespace Cubes.Server.Hubs
{
    using System.Threading.Tasks;
    using Cubes.Server.Services;
    using Shared.Join;
    using Shared.Move;
    using Shared.Spawn;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Server.Hubs;
    using UdpToolkit.Framework.Server.Rpcs;

    [Hub(hubId: 0)]
    public class CubesHub : HubBase
    {
        private readonly IGameService _gameService;

        public CubesHub(IGameService gameService)
        {
            _gameService = gameService;
        }

        [Rpc(rpcId: 0, udpChannel: UdpChannel.Udp)]
        public async Task Join(JoinEvent joinEvent)
        {
            Rooms.JoinOrCreate(joinEvent.RoomId, HubContext.PeerId);

            await Clients
                .RoomExcept(joinEvent.RoomId, HubContext.PeerId)
                .SendAsync(
                    new JoinedEvent
                    {
                        Nickname = joinEvent.Nickname,
                    },
                    HubContext)
                .ConfigureAwait(false);
        }

        [Rpc(rpcId: 1, udpChannel: UdpChannel.Udp)]
        public async Task Spawn(SpawnEvent spawnEvent)
        {
            await Clients
                .Room(spawnEvent.RoomId)
                .SendAsync(
                    new SpawnedEvent
                    {
                        PlayerId = _gameService.GetPlayerId(),
                        Position = _gameService.GetPosition(),
                        Nickname = spawnEvent.Nickname,
                    },
                    HubContext)
                .ConfigureAwait(false);
        }

        [Rpc(rpcId: 2, udpChannel: UdpChannel.Udp)]
        public async Task Move(MoveEvent moveEvent)
        {
            await Clients
                .RoomExcept(moveEvent.RoomId, HubContext.PeerId)
                .SendAsync(
                    moveEvent,
                    HubContext)
                .ConfigureAwait(false);
        }
    }
}
