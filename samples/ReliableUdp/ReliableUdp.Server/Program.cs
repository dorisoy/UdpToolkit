namespace ReliableUdp.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ReliableUdp.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
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
            var scheduler = host.ServiceProvider.Scheduler;
            var groupManager = host.ServiceProvider.GroupManager;

            host
                .On<JoinEvent>(
                    onEvent: (connectionId, ip, joinEvent) =>
                    {
                        groupManager.JoinOrCreate(joinEvent.GroupId, connectionId, ip);

                        scheduler.ScheduleOnce<StartGame>(
                            groupId: joinEvent.GroupId,
                            delay: TimeSpan.FromSeconds(3),
                            action: () => SendSpawnPoints(connectionId, joinEvent.GroupId, groupManager, broadcaster));

                        scheduler.ScheduleOnce<GameOver>(
                            groupId: joinEvent.GroupId,
                            delay: TimeSpan.FromSeconds(20),
                            action: () => broadcaster.Broadcast(
                                caller: connectionId,
                                groupId: joinEvent.GroupId,
                                @event: new GameOver(joinEvent.GroupId, "Game Over!"),
                                channelId: ReliableChannel.Id,
                                broadcastMode: BroadcastMode.Group));

                        broadcaster.Broadcast(
                            caller: connectionId,
                            groupId: joinEvent.GroupId,
                            @event: joinEvent,
                            channelId: ReliableChannel.Id,
                            broadcastMode: BroadcastMode.GroupExceptCaller);
                    });

            host
                .On<Death>(
                    onEvent: (connectionId, ip, death) =>
                    {
                        Console.WriteLine($"{death.Nickname} is dead!");

                        scheduler.Schedule<Respawn>(
                            action: () => broadcaster.Broadcast(
                                caller: connectionId,
                                groupId: death.GroupId,
                                @event: new Respawn(death.Nickname, death.GroupId),
                                channelId: ReliableChannel.Id,
                                broadcastMode: BroadcastMode.Group),
                            delay: TimeSpan.FromSeconds(1));

                        broadcaster.Broadcast(
                            caller: connectionId,
                            groupId: death.GroupId,
                            @event: death,
                            channelId: ReliableChannel.Id,
                            broadcastMode: BroadcastMode.GroupExceptCaller);
                    });

            host.Run();

            Console.ReadLine();
        }

        private static void SendSpawnPoints(
            Guid connectionId,
            Guid groupId,
            IGroupManager groupManager,
            IBroadcaster broadcaster)
        {
            var group = groupManager.GetGroup(groupId);

            var spawnPositions = group.GroupConnections
                .Select(x => x.ConnectionId)
                .Select(id => new { id, position = Positions.Dequeue() })
                .ToDictionary(pair => pair.id, pair => pair.position);

            broadcaster.Broadcast(
                caller: connectionId,
                groupId: groupId,
                @event: new StartGame(groupId, spawnPositions),
                channelId: ReliableChannel.Id,
                broadcastMode: BroadcastMode.Group);
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new NetJsonSerializer());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, settings =>
                {
                    settings.Host = "127.0.0.1";
                    settings.HostPorts = new[] { 7000, 7001 };
                    settings.Workers = 8;
                    settings.Executor = new ThreadBasedExecutor();
                    settings.LoggerFactory = new SimpleConsoleLoggerFactory(LogLevel.Debug);
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new NativeSocketFactory();
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ResendTimeout = TimeSpan.FromSeconds(120);
                    settings.ConnectionIdFactory = new ConnectionIdFactory();
                    settings.AllowIncomingConnections = true;
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
