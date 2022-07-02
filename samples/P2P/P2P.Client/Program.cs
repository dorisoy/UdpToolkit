namespace P2P.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using P2P.Contracts;
    using Serializers;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        private static readonly List<P2P.Contracts.Peer> Peers = new List<P2P.Contracts.Peer>();
        private static int _connections = 0;

        public static async Task Main(string[] args)
        {
            var nickname = args[0];
            var isConnected = false;
            var waitTimeout = TimeSpan.FromSeconds(120);

            var host = BuildHost();
            var client = host.HostClient;

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

            host.On<GroupPeers>(
                onEvent: (connectionId, ip, fetchResult) =>
                {
                    Peers.AddRange(fetchResult.Peers);

                    foreach (var peer in Peers)
                    {
                        Console.WriteLine($"Peer fetched, {peer.Address}:{peer.Port}! (event)");
                    }
                });

            host.On<Message>(
                onEvent: (connectionId, ip, message) =>
                {
                    Console.WriteLine($"P2P message received - {message.Text}! (event)");
                });

            host.Run();
            client.Connect(Guid.NewGuid());

            SpinWait.SpinUntil(() => isConnected, waitTimeout);

            client.Send(
                @event: new JoinEvent(groupId: Guid.Empty, nickname: nickname),
                channelId: ReliableChannel.Id);

            while (Peers.Count == 0)
            {
                client.Send(
                    @event: new FetchPeers(groupId: Guid.Empty, nickname: nickname),
                    channelId: ReliableChannel.Id);

                await Task.Delay(1000).ConfigureAwait(false);
            }

            foreach (var peer in Peers)
            {
                client.Connect(host: peer.Address, port: peer.Port, Guid.NewGuid());
            }

            SpinWait.SpinUntil(() => _connections == Peers.Count, waitTimeout);

            foreach (var peer in Peers)
            {
                int counter = 0;
                while (counter < 1000)
                {
                    client.Send(
                        @event: new Message(text: $"p2p message from {nickname}", groupId: Guid.Empty),
                        destination: new IpV4Address(IpUtils.ToInt(peer.Address), peer.Port),
                        channelId: ReliableChannel.Id);

                    Thread.Sleep(1000);
                    counter++;
                }
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
                    settings.HostPorts = new[] { 0 };
                    settings.Workers = 8;
                    settings.Executor = new ThreadBasedExecutor();
                    settings.ResendPacketsInterval = TimeSpan.FromSeconds(1);
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(60);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerPorts = new[] { 7000, 7001 };
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new NativeSocketFactory();
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ResendTimeout = TimeSpan.FromSeconds(120);
                    settings.AllowIncomingConnections = true;
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
