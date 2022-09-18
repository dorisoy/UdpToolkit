namespace Structs.Server
{
    using System;
    using System.Threading;
    using Serializers;
    using Structs.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        public static void Main()
        {
            var host = BuildHost();

            var broadcaster = host.ServiceProvider.Broadcaster;
            var groupManager = host.ServiceProvider.GroupManager;

            host
                .On<JoinEvent>(
                    onEvent: (connectionId, ip, joinEvent) =>
                    {
                        Console.WriteLine($"{joinEvent.Nickname} joined to room {joinEvent.GroupId}!");

                        groupManager.JoinOrCreate(joinEvent.GroupId, connectionId);

                        var roomId = joinEvent.GroupId;

                        broadcaster.ScheduleBroadcastUnmanaged<StartGame>(
                            caller: connectionId,
                            groupId: roomId,
                            timerKey: new TimerKey(roomId, typeof(StartGame)),
                            factory: GetSpawnPoints,
                            channelId: ReliableChannel.Id,
                            delay: TimeSpan.FromSeconds(10),
                            broadcastMode: BroadcastMode.Group,
                            frequency: TimeSpan.FromMilliseconds(Timeout.Infinite));

                        broadcaster.ScheduleBroadcastUnmanaged<GameOver>(
                            caller: connectionId,
                            groupId: roomId,
                            timerKey: new TimerKey(roomId, typeof(GameOver)),
                            factory: () => new GameOver(Reason.GameOver, roomId),
                            channelId: ReliableChannel.Id,
                            delay: TimeSpan.FromSeconds(20),
                            broadcastMode: BroadcastMode.Group,
                            frequency: TimeSpan.FromMilliseconds(Timeout.Infinite));

                        broadcaster.BroadcastUnmanaged<JoinEvent>(
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

                        broadcaster.ScheduleBroadcastUnmanaged<Respawn>(
                            caller: connectionId,
                            groupId: roomId,
                            timerKey: new TimerKey(Guid.NewGuid(), typeof(Respawn)),
                            factory: () => new Respawn(death.Nickname.GetHashCode(), death.GroupId),
                            channelId: ReliableChannel.Id,
                            delay: TimeSpan.FromSeconds(1),
                            broadcastMode: BroadcastMode.GroupExceptCaller,
                            frequency: TimeSpan.FromMilliseconds(Timeout.Infinite));
                    });

            host.Run();

            Console.ReadLine();
        }

        private static unsafe StartGame GetSpawnPoints()
        {
#pragma warning disable SA1129
            var sg = new StartGame();
#pragma warning restore SA1129
            var sgSpawnPositions = sg.SpawnPositions;
            sgSpawnPositions.X[0] = 123f;
            sgSpawnPositions.Y[0] = 123f;
            sgSpawnPositions.Z[0] = 123f;

            sgSpawnPositions.X[1] = 321f;
            sgSpawnPositions.Y[1] = 321f;
            sgSpawnPositions.Z[1] = 321f;

            return sg;
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new UnsafeSerializer());

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
