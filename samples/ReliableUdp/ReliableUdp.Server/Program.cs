namespace ReliableUdp.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using ReliableUdp.Contracts;
    using Serializers;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        private static readonly Queue<Position> Positions =
            new Queue<Position>(new[]
            {
                new Position(1, 1, 1),
                new Position(2, 2, 2),
                new Position(3, 3, 3),
                new Position(4, 4, 4),
                new Position(5, 5, 5),
            });

        public static void Main()
        {
            var host = BuildHost();

            var broadcaster = host.ServiceProvider.Broadcaster;
            var groupManager = host.ServiceProvider.GroupManager;

            host
                .On<JoinEvent>(
                    onEvent: (connectionId, ip, joinEvent) =>
                    {
                        Console.WriteLine($"{joinEvent.Nickname} joined!");

                        groupManager.JoinOrCreate(joinEvent.GroupId, connectionId);

                        var roomId = joinEvent.GroupId;

                        broadcaster.ScheduleBroadcast<StartGame>(
                            caller: connectionId,
                            groupId: roomId,
                            timerKey: new TimerKey(roomId, typeof(StartGame)),
                            factory: () => GetSpawnPoints(roomId, groupManager),
                            channelId: ReliableChannel.Id,
                            delay: TimeSpan.FromSeconds(10),
                            broadcastMode: BroadcastMode.Group,
                            frequency: TimeSpan.FromMilliseconds(Timeout.Infinite));

                        broadcaster.ScheduleBroadcast<GameOver>(
                            caller: connectionId,
                            groupId: roomId,
                            timerKey: new TimerKey(roomId, typeof(GameOver)),
                            factory: () => ObjectsPool<GameOver>
                                .GetOrCreate()
                                .SetUp("Game Over!", roomId),
                            channelId: ReliableChannel.Id,
                            delay: TimeSpan.FromSeconds(20),
                            broadcastMode: BroadcastMode.Group,
                            frequency: TimeSpan.FromMilliseconds(Timeout.Infinite));

                        broadcaster.Broadcast<JoinEvent>(
                            caller: connectionId,
                            groupId: roomId,
                            @event: joinEvent,
                            channelId: ReliableChannel.Id,
                            broadcastMode: BroadcastMode.GroupExceptCaller);
                    });

            host
                .On<Death>(
                    onEvent: (connectionId, ip, death) =>
                    {
                        var roomId = death.GroupId;
                        var nickname = death.Nickname;

                        Console.WriteLine($"{nickname} is dead!");

                        var respawn = ObjectsPool<Respawn>.GetOrCreate();

                        broadcaster.ScheduleBroadcast<Respawn>(
                            caller: connectionId,
                            groupId: roomId,
                            timerKey: new TimerKey(Guid.NewGuid(), typeof(Respawn)),
                            factory: () => respawn.SetUp(death.Nickname, death.GroupId),
                            channelId: ReliableChannel.Id,
                            delay: TimeSpan.FromSeconds(1),
                            broadcastMode: BroadcastMode.GroupExceptCaller,
                            frequency: TimeSpan.FromMilliseconds(Timeout.Infinite));
                    });

            host.Run();

            Console.ReadLine();
        }

        private static StartGame GetSpawnPoints(
            Guid groupId,
            IGroupManager groupManager)
        {
            var group = groupManager.GetGroup(groupId);

            var spawnPositions = group.GroupConnections
                .Select(x => x.ConnectionId)
                .Select(id => new { id, position = Positions.Dequeue() })
                .ToDictionary(pair => pair.id, pair => pair.position);

            return ObjectsPool<StartGame>
                .GetOrCreate()
                .SetUp(groupId, spawnPositions);
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new NetProtobufSerializer());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, settings =>
                {
                    settings.Host = "127.0.0.1";
                    settings.HostPorts = new[] { 7000, 7001 };
                    settings.Workers = 8;
                    settings.Executor = new ThreadBasedExecutor();
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new NativeSocketFactory();
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(15);
                    settings.ResendTimeout = TimeSpan.FromSeconds(10);
                    settings.AllowIncomingConnections = true;
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
