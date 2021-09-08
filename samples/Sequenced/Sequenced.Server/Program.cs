namespace Sequenced.Server
{
    using System;
    using Sequenced.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Executors;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Network.Channels;
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
                onEvent: (connectionId, ip, joinEvent, roomManager) =>
                {
                    roomManager
                        .JoinOrCreate(joinEvent.RoomId, connectionId, ip);

                    Log.Logger.Information($"{joinEvent.Nickname} joined to room!");

                    return joinEvent.RoomId;
                },
                broadcastMode: BroadcastMode.RoomExceptCaller);

            host.On<MoveEvent>(
                onEvent: (connectionId, ip, moveEvent) =>
                {
                    Log.Logger.Information($"Moved!");

                    return moveEvent.RoomId;
                },
                broadcastMode: BroadcastMode.RoomExceptCaller);

            host.Run();

            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new Serializer());

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
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ResendTimeout = TimeSpan.FromSeconds(120);
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new NativeSocketFactory();
                    settings.ConnectionIdFactory = new ConnectionIdFactory();
                    settings.AllowIncomingConnections = true;
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
