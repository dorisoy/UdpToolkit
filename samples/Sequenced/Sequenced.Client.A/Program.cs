namespace Sequenced.Client.A
{
    using System;
    using System.Threading.Tasks;
    using Sequenced.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Core;
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var host = BuildHost();
            var client = host.HostClient;
            var nickname = "Client A";

            host.On<JoinEvent>(
                onEvent: (connectionId, joinEvent) =>
                {
                    Log.Logger.Information($"{joinEvent.Nickname} joined to room!");
                    return joinEvent.RoomId;
                },
                onAck: (connectionId) =>
                {
                    Log.Logger.Information($"{nickname} joined to room!");
                },
                broadcastMode: BroadcastMode.RoomExceptCaller,
                hookId: 0);

            host.On<MoveEvent>(
                onEvent: (connectionId, move) =>
                {
                    Log.Debug($"Id {move.Id} - from - {move.From}");
                    return move.RoomId;
                },
                hookId: 1,
                broadcastMode: BroadcastMode.RoomExceptCaller);

#pragma warning disable CS4014
            Task.Run(() => host.RunAsync());
#pragma warning restore CS4014

            var isConnected = client
                .Connect();

            client.Send(
                @event: new JoinEvent(
                    roomId: 0,
                    nickname: nickname),
                hookId: 0,
                udpMode: UdpMode.ReliableUdp);

            await Task.Delay(20_000).ConfigureAwait(false);

            for (var i = 0; i < 5; i++)
            {
                client.Send(
                    @event: new MoveEvent(
                        id: i,
                        roomId: 0,
                        from: nickname),
                    hookId: 1,
                    udpMode: UdpMode.Sequenced);
            }

            Console.WriteLine($"IsConnected - {isConnected}");

            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost((settings) =>
                {
                    settings.Host = "127.0.0.1";
                    settings.Serializer = new Serializer();
                    settings.LoggerFactory = new SerilogLoggerFactory();
                    settings.InputPorts = new[] { 3000, 3001 };
                    settings.OutputPorts = new[] { 4000, 4001 };
                    settings.Workers = 2;
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.InactivityTimeout = TimeSpan.FromSeconds(120);
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerInputPorts = new[] { 7000, 7001 };
                    settings.HeartbeatDelayInMs = 1000; // pass null for disable heartbeat
                })
                .Build();
        }
    }
}