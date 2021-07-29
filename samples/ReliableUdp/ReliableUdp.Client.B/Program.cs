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
    using UdpToolkit.Core.Executors;
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

            var cts = new CancellationTokenSource();
            Task.Run(
                async () =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        if (client.IsConnected)
                        {
                            Log.Logger.Debug($"RTT - {client.Rtt.TotalMilliseconds}");
                        }

                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                }, cts.Token);

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
                hookId: 0);

            host.Run();

            client.Connect();

            var waitTimeout = TimeSpan.FromSeconds(120);
            SpinWait.SpinUntil(() => client.IsConnected, waitTimeout);
            Console.WriteLine($"IsConnected - {client.IsConnected}");

            client.Send(
                @event: new JoinEvent(roomId: 11, nickname: nickname),
                hookId: 0,
                udpMode: UdpMode.ReliableUdp);

            Console.WriteLine("Press any key...");
            Console.ReadLine();

            cts.Cancel();
            host.Dispose();
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
                    settings.HostPorts = new[] { 5000, 5001 };
                    settings.Workers = 8;
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.ConnectionTtl = TimeSpan.FromSeconds(120);
                    settings.ExecutorType = ExecutorType.ThreadBasedExecutor;
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerPorts = new[] { 7000, 7001 };
                    settings.HeartbeatDelayInMs = 1000; // pass null for disable heartbeat
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.SocketType = SocketType.Native;
                })
                .Build();
        }
    }
}
