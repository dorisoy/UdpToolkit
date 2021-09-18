namespace P2P.Server
{
    using System;
    using System.Linq;
    using P2P.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        public static void Main()
        {
            var host = BuildHost();
            var roomManager = host.ServiceProvider.RoomManager;
            var broadcaster = host.ServiceProvider.Broadcaster;

            host.On<JoinEvent>(
                onEvent: (connectionId, ip, joinEvent) =>
                {
                    roomManager.JoinOrCreate(joinEvent.RoomId, connectionId, ip);

                    Console.WriteLine($"{joinEvent.Nickname} joined to room!");

                    broadcaster.Broadcast(
                        caller: connectionId,
                        roomId: joinEvent.RoomId,
                        @event: joinEvent,
                        channelId: ReliableChannel.Id,
                        broadcastMode: BroadcastMode.RoomExceptCaller);
                });

            host
                .On<FetchPeers>(
                    onEvent: (connectionId, ip, fetchPeers) =>
                    {
                        var room = roomManager.GetRoom(fetchPeers.RoomId);

                        var peers = room.RoomConnections
                            .Where(x => x.ConnectionId != connectionId)
                            .Select(x => new Peer(x.IpV4Address.Address.ToHost(), x.IpV4Address.Port))
                            .ToList();

                        Console.WriteLine($"{fetchPeers.Nickname} Fetch peers!");

                        broadcaster.Broadcast(
                            caller: connectionId,
                            roomId: fetchPeers.RoomId,
                            @event: new RoomPeers(fetchPeers.RoomId, peers),
                            channelId: ReliableChannel.Id,
                            broadcastMode: BroadcastMode.Caller);
                    });

            host.Run();

            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new MessagePackSerializer());

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
