namespace ReliableUdp.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ReliableUdp.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Executors;
    using UdpToolkit.Framework.Contracts.Settings;
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

            host
                .On<JoinEvent>(
                    onEvent: (connectionId, ip, joinEvent) =>
                    {
                        host.ServiceProvider.RoomManager
                            .JoinOrCreate(joinEvent.RoomId, connectionId, ip);

                        host.ServiceProvider.Scheduler
                            .Schedule<JoinEvent>(
                                inEvent: joinEvent,
                                caller: connectionId,
                                timerKey: new TimerKey(joinEvent.RoomId, typeof(StartGame)),
                                dueTime: TimeSpan.FromMilliseconds(20_000),
                                action: SendSpawnPoints);

                        host.ServiceProvider.Scheduler
                            .Schedule<JoinEvent>(
                                inEvent: joinEvent,
                                caller: connectionId,
                                timerKey: new TimerKey(joinEvent.RoomId, typeof(GameOver)),
                                dueTime: TimeSpan.FromMilliseconds(40_000),
                                action: SendGameOver);

                        host.ServiceProvider.Broadcaster
                            .Broadcast(
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

                        host.ServiceProvider.Scheduler
                            .Schedule<Death>(
                                inEvent: death,
                                caller: connectionId,
                                timerKey: new TimerKey(Guid.NewGuid(), typeof(Respawn)),
                                dueTime: TimeSpan.FromMilliseconds(20_000),
                                action: SendRespawn);

                        host.ServiceProvider.Broadcaster
                            .Broadcast(
                                caller: connectionId,
                                roomId: death.RoomId,
                                @event: death,
                                channelId: ReliableChannel.Id,
                                broadcastMode: BroadcastMode.RoomExceptCaller);
                    });

            host.Run();

            Console.ReadLine();
        }

        private static void SendRespawn(
            Guid connectionId,
            Death death,
            IRoomManager roomManager,
            IBroadcaster broadcaster)
        {
            broadcaster
                .Broadcast(
                    caller: connectionId,
                    roomId: death.RoomId,
                    @event: new Respawn(death.Nickname, death.RoomId),
                    channelId: ReliableChannel.Id,
                    broadcastMode: BroadcastMode.Room);
        }

        private static void SendGameOver(
            Guid connectionId,
            JoinEvent joinEvent,
            IRoomManager roomManager,
            IBroadcaster broadcaster)
        {
            broadcaster
                .Broadcast(
                    caller: connectionId,
                    roomId: joinEvent.RoomId,
                    @event: new GameOver(joinEvent.RoomId, "Game Over!"),
                    channelId: ReliableChannel.Id,
                    broadcastMode: BroadcastMode.Room);
        }

        private static void SendSpawnPoints(
            Guid connectionId,
            JoinEvent joinEvent,
            IRoomManager roomManager,
            IBroadcaster broadcaster)
        {
            var room = roomManager.GetRoom(joinEvent.RoomId);

            var spawnPositions = room.RoomConnections
                .Select(x => x.ConnectionId)
                .Select(id => new { id, position = Positions.Dequeue() })
                .ToDictionary(pair => pair.id, pair => pair.position);

            broadcaster
                .Broadcast(
                    caller: connectionId,
                    roomId: joinEvent.RoomId,
                    @event: new StartGame(joinEvent.RoomId, spawnPositions),
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
                    settings.LoggerFactory = new SimpleConsoleLoggerFactory(LogLevel.Error);
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
