namespace ReliableUdp.Client.A
{
    using System;
    using System.Linq;
    using System.Threading;
    using ReliableUdp.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Executors;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        public static void Main()
        {
            var host = BuildHost();
            var client = host.HostClient;

            var nickname = "Client A";

            var isConnected = false;

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
                onEvent: (connectionId, ip, startGame, roomManager) =>
                {
                    roomManager
                        .JoinOrCreate(startGame.RoomId, connectionId, ip);

                    var positions = startGame.Positions
                        .Select(pair => pair.Key + "|" + pair.Value.X + pair.Value.Y + pair.Value.Z)
                        .ToArray();

                    Console.WriteLine($"Spawn positions - {positions}!", positions.Length);
                    return startGame.RoomId;
                });

            host.Run();

            client.Connect();

            var waitTimeout = TimeSpan.FromSeconds(120);
            SpinWait.SpinUntil(() => isConnected, waitTimeout);
            Console.WriteLine($"IsConnected - {isConnected}");

            client.Send(
                @event: new JoinEvent(roomId: 11, nickname: nickname),
                channelId: ReliableChannel.Id);

            Console.WriteLine("Press any key...");
            Console.ReadLine();
            client.Disconnect();
            SpinWait.SpinUntil(() => !isConnected, waitTimeout);
            Console.WriteLine($"!Disconnected - {isConnected}");

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
                    settings.HostPorts = new[] { 3000, 3001 };
                    settings.Workers = 8;
                    settings.Executor = new TaskBasedExecutor();
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
