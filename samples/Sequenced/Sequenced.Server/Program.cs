namespace Sequenced.Server
{
    using System;
    using Sequenced.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Core;
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

            host.On<MoveEvent>(
                onEvent: (connectionId, moveEvent) =>
                {
                    Log.Logger.Information("Moved!");

                    return moveEvent.RoomId;
                },
                broadcastMode: BroadcastMode.RoomExceptCaller,
                hookId: 1);

            host.Run();
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
                    settings.ConnectionTtl = TimeSpan.FromSeconds(120);
                })
                .Build();
    }
}
