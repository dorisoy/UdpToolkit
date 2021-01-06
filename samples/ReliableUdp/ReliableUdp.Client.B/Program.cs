namespace ReliableUdp.Client.B
{
    using System;
    using System.Threading.Tasks;
    using ReliableUdp.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;
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
            var client = host.ServerHostClient;
            var nickname = "Client B";

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
                onTimeout: (peerId) => { },
                broadcastMode: BroadcastMode.Room,
                hookId: 0);

            host.On<StartGame>(
                onEvent: (peerId, startGame) =>
                {
                    Log.Logger.Information("Game started!");

                    return startGame.RoomId;
                },
                onTimeout: (peerId) => { },
                onAck: (peerId) => { },
                broadcastMode: BroadcastMode.Room,
                hookId: 1);

#pragma warning disable CS4014
            Task.Run(() => host.RunAsync());
#pragma warning restore CS4014

            var isConnected = client
                .Connect();

            client.Publish(
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
                    settings.InputPorts = new[] { 5000, 5001 };
                    settings.OutputPorts = new[] { 6000, 6001 };
                    settings.Workers = 2;
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.PeerInactivityTimeout = TimeSpan.FromSeconds(120);
                })
                .ConfigureServerHostClient((settings) =>
                {
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ClientHost = "127.0.0.1";
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerInputPorts = new[] { 7000, 7001 };
                    settings.PingDelayInMs = null; // pass null for disable pings
                })
                .Build();
        }
    }
}
