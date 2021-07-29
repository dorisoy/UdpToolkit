namespace P2P.Server
{
    using System;
    using System.Linq;
    using P2P.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Core;
    using UdpToolkit.Core.Executors;
    using UdpToolkit.Logging.Serilog;
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
                        .Select(x => new Peer(x.IpV4Address.Host, x.IpV4Address.Port))
                        .ToList();

                    Log.Logger.Information($"{fetchPeers.Nickname} Fetch peers!");

                    return (
                        roomId: fetchPeers.RoomId,
                        response: new RoomPeers(fetchPeers.RoomId, peers),
                        delayInMs: 0,
                        broadcastMode: BroadcastMode.Caller,
                        uppMode: UdpMode.ReliableUdp);
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
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.ConnectionTtl = TimeSpan.FromSeconds(30);
                    settings.ExecutorType = ExecutorType.ThreadBasedExecutor;
                })
                .Build();
    }
}
