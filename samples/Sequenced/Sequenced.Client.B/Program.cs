namespace Sequenced.Client.B
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Sequenced.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = BuildHost();
            var client = host.HostClient;
            var nickname = "Client B";

            var isConnected = false;

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

            host.On<JoinEvent>(
                onEvent: (connectionId, ip, joinEvent) =>
                {
                    Console.WriteLine($"{joinEvent.Nickname} joined to group!");
                });

            host.On<MoveEvent>(
                onEvent: (connectionId, ip, move) =>
                {
                    Console.WriteLine($"Id {move.Id} - from - {move.From}");
                });

            host.Run();

            client.Connect();

            var waitTimeout = TimeSpan.FromSeconds(120);
            SpinWait.SpinUntil(() => isConnected, waitTimeout);
            Console.WriteLine($"IsConnected - {isConnected}");

            client.Send(
                @event: new JoinEvent(
                    groupId: Guid.Empty,
                    nickname: nickname),
                channelId: ReliableChannel.Id);

            await Task.Delay(20_000).ConfigureAwait(false);

            for (var i = 0; i < 5000; i++)
            {
                client.Send(
                    @event: new MoveEvent(
                        id: i,
                        groupId: Guid.Empty,
                        from: nickname),
                    channelId: SequencedChannel.Id);
                Thread.Sleep(1000 / 60);
            }

            client.Disconnect();
            SpinWait.SpinUntil(() => !isConnected);
            Console.WriteLine($"Client disconnected, IsConnected - {isConnected}");

            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new MessagePackSerializer());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, (settings) =>
                {
                    settings.Host = "127.0.0.1";
                    settings.HostPorts = new[] { 5000, 5001 };
                    settings.Workers = 8;
                    settings.Executor = new ThreadBasedExecutor();
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerPorts = new[] { 7000, 7001 };
                    settings.HeartbeatDelayInMs = 1000; // pass null for disable heartbeat
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new NativeSocketFactory();
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ResendTimeout = TimeSpan.FromSeconds(120);
                    settings.ConnectionIdFactory = new ConnectionIdFactory();
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}