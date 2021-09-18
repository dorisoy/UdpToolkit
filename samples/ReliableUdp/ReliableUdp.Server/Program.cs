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
            var roomManager = host.ServiceProvider.RoomManager;

            host
                .On<JoinEvent>(
                    onEvent: (connectionId, ip, joinEvent) =>
                    {
                        roomManager.JoinOrCreate(joinEvent.RoomId, connectionId, ip);

                        scheduler.ScheduleOnce<StartGame>(
                            roomId: joinEvent.RoomId,
                            delay: TimeSpan.FromSeconds(3),
                            action: () => SendSpawnPoints(connectionId, joinEvent.RoomId, roomManager, broadcaster));

                        scheduler.ScheduleOnce<GameOver>(
                            roomId: joinEvent.RoomId,
                            delay: TimeSpan.FromSeconds(20),
                            action: () => broadcaster.Broadcast(
                                caller: connectionId,
                                roomId: joinEvent.RoomId,
                                @event: new GameOver(joinEvent.RoomId, "Game Over!"),
                                channelId: ReliableChannel.Id,
                                broadcastMode: BroadcastMode.Room));

                        broadcaster.Broadcast(
                            caller: connectionId,
                            roomId: joinEvent.RoomId,
                            @event: joinEvent,
                            channelId: ReliableChannel.Id,
                            broadcastMode: BroadcastMode.RoomExceptCaller);
                    });

            host
                .On<Death>(
                    onEvent: (connectionId, ip, death) =>
                    {
                        Console.WriteLine($"{death.Nickname} is dead!");

                        scheduler.Schedule<Respawn>(
                            action: () => broadcaster.Broadcast(
                                caller: connectionId,
                                roomId: death.RoomId,
                                @event: new Respawn(death.Nickname, death.RoomId),
                                channelId: ReliableChannel.Id,
                                broadcastMode: BroadcastMode.Room),
                            delay: TimeSpan.FromSeconds(1));

                        broadcaster.Broadcast(
                            caller: connectionId,
                            roomId: death.RoomId,
                            @event: death,
                            channelId: ReliableChannel.Id,
                            broadcastMode: BroadcastMode.RoomExceptCaller);
                    });

            host.Run();

            Console.ReadLine();
        }

        private static void SendSpawnPoints(
            Guid connectionId,
            Guid roomId,
            IRoomManager roomManager,
            IBroadcaster broadcaster)
        {
            var room = roomManager.GetRoom(roomId);

            var spawnPositions = room.RoomConnections
                .Select(x => x.ConnectionId)
                .Select(id => new { id, position = Positions.Dequeue() })
                .ToDictionary(pair => pair.id, pair => pair.position);

            broadcaster.Broadcast(
                caller: connectionId,
                roomId: roomId,
                @event: new StartGame(roomId, spawnPositions),
                channelId: ReliableChannel.Id,
                broadcastMode: BroadcastMode.Room);
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
