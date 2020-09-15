namespace SimpleUdp.Server
{
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using SimpleUdp.Contracts;
    using UdpToolkit.Core;
    using UdpToolkit.Framework;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var host = BuildHost();

            host.On<JoinEvent, JoinedEvent>(
                handler: (peerId, joinEvent, roomManager, builder) =>
                {
                    roomManager.JoinOrCreate(joinEvent.RoomId, peerId);

                    Log.Logger.Information($"{joinEvent.Nickname} joined to room!");

                    return builder.Room(new JoinedEvent(joinEvent.Nickname), joinEvent.RoomId, 1);
                },
                hookId: 0);

            await host
                .RunAsync()
                .ConfigureAwait(false);
        }

        private static IHost BuildHost() =>
            UdpHost
                .CreateHostBuilder()
                .ConfigureHost(settings =>
                {
                    settings.Host = "0.0.0.0";
                    settings.InputPorts = new[] { 7000, 7001 };
                    settings.OutputPorts = new[] { 8000, 8001 };
                    settings.Workers = 2;
                    settings.Serializer = new Serializer();
                })
                .ConfigureServerHostClient((settings) =>
                {
                    settings.ServerHost = "0.0.0.0";
                    settings.ServerPorts = new[] { 7000, 7001 };
                })
                .Build();
    }
}
