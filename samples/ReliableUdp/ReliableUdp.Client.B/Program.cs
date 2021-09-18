namespace ReliableUdp.Client.B
{
    using System;
    using System.Linq;
    using System.Threading;
    using ReliableUdp.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        private static bool _isStarted = false;
        private static bool _isOver = false;

        public static void Main()
        {
            var host = BuildHost();
            var client = host.HostClient;

            var nickname = "Client B";

            var isConnected = false;
            var roomManager = host.ServiceProvider.RoomManager;

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

                    roomManager.JoinOrCreate(startGame.RoomId, connectionId, ip);

                    var positions = startGame.Positions
                        .Select(pair => $"{pair.Key}|X - {pair.Value.X}|Y - {pair.Value.Y}|Z - {pair.Value.Z}");

                    foreach (var position in positions)
                    {
                        Console.WriteLine($"Spawn positions - {position}!");
                    }
                });

            host.On<GameOver>(
                onEvent: (connectionId, ip, gameOver) =>
                {
                    Console.WriteLine("End of Game!");
                    _isOver = true;
                });

            host.On<Respawn>(
                onEvent: (connectionId, ip, respawn) =>
                {
                    Console.WriteLine($"Respawn for {respawn.Nickname}!");
                });

            host.Run();

            client.Connect();

            var waitTimeout = TimeSpan.FromSeconds(120);
            SpinWait.SpinUntil(() => isConnected, waitTimeout);
            Console.WriteLine($"IsConnected - {isConnected}");

            client.Send(
                @event: new JoinEvent(roomId: Guid.Empty, nickname: nickname),
                channelId: ReliableChannel.Id);

            SpinWait.SpinUntil(() => _isStarted, waitTimeout);
            Console.WriteLine($"Game started!");

            for (var i = 0; i < 3; i++)
            {
                client.Send(
                    @event: new Death(nickname, Guid.Empty),
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
                serializer: new NetJsonSerializer());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, (settings) =>
                {
                    settings.Host = "127.0.0.1";
                    settings.HostPorts = new[] { 5000, 5001 };
                    settings.Workers = 8;
                    settings.Executor = new TaskBasedExecutor();
                    settings.LoggerFactory = new SimpleConsoleLoggerFactory(LogLevel.Error);
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(15);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerPorts = new[] { 7000, 7001 };
                    settings.HeartbeatDelayInMs = 1000; // pass null for disable heartbeat
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new ManagedSocketFactory();
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ResendTimeout = TimeSpan.FromSeconds(20);
                    settings.ConnectionIdFactory = new ConnectionIdFactory();
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
