namespace ReliableUdp.Client.A
{
    using System;
    using System.Linq;
    using System.Threading;
    using ReliableUdp.Contracts;
    using Serializers;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        private static readonly Guid ConnectionId = Guid.NewGuid();
        private static bool _isStarted = false;
        private static bool _isOver = false;

        public static void Main()
        {
            var host = BuildHost();
            var client = host.HostClient;

            var nickname = "Client A";

            var isConnected = false;
            var groupManager = host.ServiceProvider.GroupManager;

            host.HostClient.OnConnectionTimeout += () =>
            {
                Console.WriteLine($"ConnectionTimeout for - {nickname}");
                isConnected = false;
            };
            host.HostClient.OnRttReceived += rtt => Console.WriteLine($"{nickname} rtt - {rtt}");
            host.HostClient.OnConnected += (ipV4, connectionId) =>
            {
                isConnected = true;
                Console.WriteLine($"{nickname} connected with id - {connectionId}");
            };
            host.HostClient.OnDisconnected += (ipV4, connectionId) =>
            {
                isConnected = false;
                Console.WriteLine($"{nickname} disconnected with id - {connectionId}");
            };

            host.On<StartGame>(
                onEvent: (connectionId, ip, startGame) =>
                {
                    _isStarted = true;

                    groupManager.JoinOrCreate(startGame.GroupId, connectionId, ip);

                    var positions = startGame.Positions
                        .Select(pair => $"{pair.Key}|X - {pair.Value.X}|Y - {pair.Value.Y}|Z - {pair.Value.Z}");

                    foreach (var position in positions)
                    {
                        Console.WriteLine($"Spawn positions - {position}!");
                    }

                    return startGame.GroupId;
                });

            host.On<GameOver>(
                onEvent: (connectionId, ip, gameOver) =>
                {
                    Console.WriteLine("End of Game!");
                    _isOver = true;

                    return gameOver.GroupId;
                });

            host.On<Respawn>(
                onEvent: (connectionId, ip, respawn) =>
                {
                    Console.WriteLine($"Respawn for {respawn.Nickname}!");

                    return respawn.GroupId;
                });

            host.Run();

            client.Connect(ConnectionId);

            var waitTimeout = TimeSpan.FromSeconds(120);
            SpinWait.SpinUntil(() => isConnected, waitTimeout);
            Console.WriteLine($"IsConnected - {isConnected}");

            var joinEvent = ObjectsPool<JoinEvent>.GetOrCreate();
            client.Send(
                @event: joinEvent.Set(Guid.Empty, nickname),
                channelId: ReliableChannel.Id);

            SpinWait.SpinUntil(() => _isStarted, waitTimeout);
            Console.WriteLine($"Game started!");

            for (var i = 0; i < 3; i++)
            {
                var deathEvent = ObjectsPool<Death>.GetOrCreate();
                client.Send(
                    @event: deathEvent.Set(Guid.Empty, nickname),
                    channelId: ReliableChannel.Id);

                Thread.Sleep(1000);
            }

            SpinWait.SpinUntil(() => _isOver, waitTimeout);
            Console.WriteLine($"Game over!");

            client.Disconnect();
            SpinWait.SpinUntil(() => !isConnected, waitTimeout);
            Console.WriteLine($"Client closed!");

            host.Dispose();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new NetProtobufSerializer());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, (settings) =>
                {
                    settings.Host = "127.0.0.1";
                    settings.HostPorts = new[] { 3000, 3001 };
                    settings.Workers = 8;
                    settings.Executor = new TaskBasedExecutor();
                    settings.LoggerFactory = new SimpleConsoleLoggerFactory(LogLevel.Information);
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(150);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerPorts = new[] { 7000, 7001 };
                    settings.HeartbeatDelayInMs = 1000; // pass null for disable heartbeat
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new NativeSocketFactory();
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ResendTimeout = TimeSpan.FromSeconds(20);
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}