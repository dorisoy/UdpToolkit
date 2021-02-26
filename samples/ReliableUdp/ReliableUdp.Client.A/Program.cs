namespace ReliableUdp.Client.A
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using ReliableUdp.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Core;
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Protocol;
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
            var client = host.HostClient;
            var nickname = "Client A";

            host.OnProtocol<Connect>(
                onProtocolEvent: (peerId, connected) =>
                {
                    Log.Logger.Information($"Must be raised only on server side");
                },
                onAck: (peerId) =>
                {
                    Log.Logger.Information($"{nickname} connected with peerId - {peerId}");
                },
                onTimeout: (peerId) =>
                {
                    Log.Logger.Information($"Connection timeout - {peerId}");
                },
                protocolHookId: ProtocolHookId.Connect);

            host.On<JoinEvent>(
                onEvent: (peerId, joinEvent) =>
                {
                    Log.Logger.Information($"{joinEvent.Nickname} joined to room!");
                    return joinEvent.RoomId;
                },
                onAck: (peerId) =>
                {
                    Log.Logger.Information($"{nickname} joined to room!");
                },
                broadcastMode: BroadcastMode.Room,
                hookId: 0);

            host.On<StartGame>(
                onEvent: (peerId, startGame) =>
                {
                    var positions = startGame.Positions
                        .Select(pair => pair.Key + "|" + pair.Value.x + pair.Value.y + pair.Value.z)
                        .ToArray();

                    Log.Logger.Information("Spawn positions - {@positions}!", positions);
                    return startGame.RoomId;
                },
                broadcastMode: BroadcastMode.Room,
                hookId: 1);

#pragma warning disable CS4014
            Task.Run(() => host.RunAsync());
#pragma warning restore CS4014

            var isConnected = client
                .Connect();

            client.Send(
                @event: new JoinEvent(roomId: 11, nickname: nickname),
                hookId: 0,
                udpMode: UdpMode.ReliableUdp);

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
                    settings.PeerInactivityTimeout = TimeSpan.FromSeconds(120);
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerInputPorts = new[] { 7000, 7001 };
                    settings.PingDelayInMs = null; // pass null for disable pings
                })
                .Build();
        }
    }
}
