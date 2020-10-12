namespace SimpleUdp.Client
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using SimpleUdp.Contracts;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Framework;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        public static void Main()
        {
            var ip1 = IPEndPoint.Parse("0.0.0.0:5000");
            var ip2 = IPEndPoint.Parse("127.0.0.1:5000");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var host = BuildHost();
            var client = host.ServerHostClient;
            var nickname = "keygen";

            host.OnProtocol<Connect>(
                onEvent: (peerId, connected) =>
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
                onEvent: (peerId, joinEvent) => { },
                onAck: (peerId) =>
                {
                    Log.Logger.Information($"{nickname} joined to room!");
                },
                broadcastMode: BroadcastMode.Room,
                hookId: 0);

            host.On<StartGame>(
                onEvent: (peerId, startGame) =>
                {
                    Log.Logger.Information("Game started!");
                },
                broadcastMode: BroadcastMode.Room,
                hookId: 1);

#pragma warning disable
            Task.Run(() => host.RunAsync());
#pragma warning restore

            var isConnected = client
                .Connect();

            client.Publish(
                @event: new JoinEvent(roomId: 0, nickname: "keygen"),
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
                    settings.InputPorts = new[] { 5000, 5001 };
                    settings.OutputPorts = new[] { 6000, 6001 };
                    settings.Workers = 2;
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.PeerInactivityTimeout = TimeSpan.FromSeconds(60);
                })
                .ConfigureServerHostClient((settings) =>
                {
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(5);
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ClientHost = "127.0.0.1";
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerInputPorts = new[] { 7000, 7001 };
                    settings.ServerOutputPorts = new[] { 8000, 8001 };
                    settings.PingDelayInMs = null; // pass null for disable pings
                })
                .Build();
        }
    }
}
