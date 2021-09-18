namespace P2P.Client.A
{
    using System;
    using System.Threading;
    using P2P.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        private static readonly string Host = "127.0.0.1";
        private static readonly int Port = 5000;
        private static int _connections = 0;

        public static void Main()
        {
            var nickname = "ClientA";
            var host = BuildHost();
            var client = host.HostClient;

            var isConnected = false;

            host.HostClient.OnRttReceived += rtt => Console.WriteLine($"{nickname} rtt - {rtt}");
            host.HostClient.OnConnected += (ipV4, connectionId) =>
            {
                _connections++;
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
                    Console.WriteLine($"{joinEvent.Nickname} joined to group! (event)");
                });

            host.On<Message>(
                onEvent: (connectionId, ip, message) =>
                {
                    Console.WriteLine($"P2P message received - {message.Text}! (event)");
                });

            host.Run();
            client.Connect();

            var waitTimeout = TimeSpan.FromSeconds(120);
            SpinWait.SpinUntil(() => isConnected, waitTimeout);

            client.Send(
                @event: new JoinEvent(groupId: Guid.Empty, nickname: nickname),
                channelId: ReliableChannel.Id);

            client.Connect(host: Host, port: Port);

            SpinWait.SpinUntil(() => _connections == 2, waitTimeout);

            int counter = 0;
            while (counter < 1000)
            {
                client.Send(
                    @event: new Message(text: $"p2p message from {nickname}", groupId: Guid.Empty),
                    destination: new IpV4Address(Host.ToInt(), (ushort)Port),
                    channelId: ReliableChannel.Id);

                Thread.Sleep(1000);
                counter++;
            }

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
                    settings.HostPorts = new[] { 3000, 3001 };
                    settings.Workers = 8;
                    settings.Executor = new ThreadBasedExecutor();
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(60);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerPorts = new[] { 7000, 7001 };
                    settings.HeartbeatDelayInMs = 1000; // pass null for disable heartbeat
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new ManagedSocketFactory();
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
