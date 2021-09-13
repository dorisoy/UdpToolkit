namespace P2P.Server
{
    using System;
    using System.Linq;
    using P2P.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Executors;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var host = BuildHost();

            host.On<JoinEvent>(
                onEvent: (connectionId, ip, joinEvent) =>
                {
                    host.ServiceProvider.RoomManager
                        .JoinOrCreate(joinEvent.RoomId, connectionId, ip);

                    Log.Logger.Information($"{joinEvent.Nickname} joined to room!");

                    host.ServiceProvider.Broadcaster
                        .Broadcast(
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
                        var room = host.ServiceProvider.RoomManager
                            .GetRoom(fetchPeers.RoomId);

                        var peers = room.RoomConnections
                            .Where(x => x.ConnectionId != connectionId)
                            .Select(x => new Peer(x.IpV4Address.Address.ToHost(), x.IpV4Address.Port))
                            .ToList();

                        Log.Logger.Information($"{fetchPeers.Nickname} Fetch peers!");

                        host.ServiceProvider.Broadcaster
                            .Broadcast(
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
                    settings.LoggerFactory = new SerilogLoggerFactory();
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
