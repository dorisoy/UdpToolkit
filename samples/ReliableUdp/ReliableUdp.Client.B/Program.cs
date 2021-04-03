namespace ReliableUdp.Client.B
{
    using System;
    using System.Linq;
    using System.Threading;
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
            client.OnConnectionTimeout += () => { Console.WriteLine("Connection Timeout"); };
            var nickname = "Client B";

            Task.Run(async () =>
            {
                while (true)
                {
                    if (client.IsConnected)
                    {
                        Log.Logger.Debug($"RTT - {client.Rtt.TotalMilliseconds}");
                    }

                    await Task.Delay(1000).ConfigureAwait(false);
                }
            });

            host.OnProtocol<Connect>(
                onProtocolEvent: (connectionId, connected) =>
                {
                    Log.Logger.Information($"Must be raised only on server side");
                },
                onAck: (connectionId) =>
                {
                    Log.Logger.Information($"{nickname} connected with connectionId - {connectionId}");
                },
                onTimeout: (connectionId) =>
                {
                    Log.Logger.Information($"Connection timeout - {connectionId}");
                },
                protocolHookId: ProtocolHookId.Connect);

            host.On<JoinEvent>(
                onEvent: (connectionId, joinEvent) =>
                {
                    Log.Logger.Information($"{joinEvent.Nickname} joined to room! (event)");
                    return joinEvent.RoomId;
                },
                onAck: (connectionId) =>
                {
                    Log.Logger.Information($"{nickname} joined to room! (ack)");
                },
                onTimeout: (connectionId) => { },
                broadcastMode: BroadcastMode.Room,
                hookId: 0);

            host.On<StartGame>(
                onEvent: (connectionId, startGame) =>
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
            Task.Run(() => host.Run());
#pragma warning restore CS4014

            client.Connect();

            var connectionTimeout = TimeSpan.FromSeconds(120);
            SpinWait.SpinUntil(() => client.IsConnected, connectionTimeout);
            Console.WriteLine($"IsConnected - {client.IsConnected}");

            client.Send(
                @event: new JoinEvent(roomId: 11, nickname: nickname),
                hookId: 0,
                udpMode: UdpMode.ReliableUdp);

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
                    settings.InputPorts = new[] { 5000, 5001 };
                    settings.OutputPorts = new[] { 6000, 6001 };
                    settings.Workers = 2;
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.ConnectionTtl = TimeSpan.FromSeconds(120);
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerInputPorts = new[] { 7000, 7001 };
                    settings.HeartbeatDelayInMs = 1000; // pass null for disable heartbeat
                })
                .Build();
        }
    }
}
