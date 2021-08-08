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
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;
    using UdpToolkit.Serialization.MsgPack;

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
                onEvent: (connectionId, joinEvent, roomManager) =>
                {
                    roomManager
                        .JoinOrCreate(joinEvent.RoomId, connectionId);

                    Log.Logger.Information($"{joinEvent.Nickname} joined to room!");

                    return joinEvent.RoomId;
                },
                broadcastMode: BroadcastMode.RoomExceptCaller,
                hookId: 0);

            host.On<FetchPeers, RoomPeers>(
                onEvent: (connectionId, fetchPeers, roomManager) =>
                {
                    var room = roomManager
                        .GetRoom(fetchPeers.RoomId);

                    var peers = room.RoomConnections
                        .Where(x => x.ConnectionId != connectionId)
                        .Select(x => new Peer(x.IpV4Address.Address.ToHost(), x.IpV4Address.Port))
                        .ToList();

                    Log.Logger.Information($"{fetchPeers.Nickname} Fetch peers!");

                    return (
                        roomId: fetchPeers.RoomId,
                        response: new RoomPeers(fetchPeers.RoomId, peers),
                        delayInMs: 0,
                        broadcastMode: BroadcastMode.Caller,
                        channelId: ReliableChannel.Id);
                },
                broadcastMode: BroadcastMode.None,
                hookId: 1);

            host.Run();

            Console.ReadLine();
        }

        private static IHost BuildHost() =>
            UdpHost
                .CreateHostBuilder()
                .ConfigureHost(settings =>
                {
                    settings.Host = "127.0.0.1";
                    settings.Serializer = new Serializer();
                    settings.LoggerFactory = new SerilogLoggerFactory();
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
                })
                .Build();
    }
}
