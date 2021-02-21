namespace ReliableUdp.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ReliableUdp.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Core;
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Serialization.MsgPack;
    using UnityEngine;

    public static class Program
    {
        private static readonly Queue<Vector3> Positions =
            new Queue<Vector3>(new[]
            {
                new Vector3(1, 1, 1),
                new Vector3(2, 2, 2),
                new Vector3(3, 3, 3),
                new Vector3(4, 4, 4),
                new Vector3(5, 5, 5),
            });

        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var host = BuildHost();

            host.On<JoinEvent>(
                onEvent: (peerId, joinEvent, roomManager) =>
                {
                    roomManager
                        .JoinOrCreate(joinEvent.RoomId, peerId);

                    Log.Logger.Information($"{joinEvent.Nickname} joined to room!");

                    return joinEvent.RoomId;
                },
                scheduleCall: new ScheduledCall<JoinEvent>(
                    timerId: Timers.JoinTimeout,
                    action: (peerId, joinEvent, roomManager) =>
                    {
                        var peers = roomManager
                            .GetRoomPeers(joinEvent.RoomId);

                        var spawnPositions = peers
                            .Select(id => new { id, position = Positions.Dequeue() })
                            .ToDictionary(pair => pair.id, pair => pair.position);

                        Log.Logger.Information($"Scheduled event!");
                        host.SendCore(
                            @event: new StartGame(joinEvent.RoomId, spawnPositions),
                            roomId: joinEvent.RoomId,
                            hookId: 1,
                            udpMode: UdpMode.ReliableUdp,
                            broadcastMode: BroadcastMode.Room);
                    },
                    delay: TimeSpan.FromMilliseconds(20_000)),
                broadcastMode: BroadcastMode.Room,
                hookId: 0);

            host.On<StartGame>(
                onEvent: (peerId, startGame) =>
                {
                    Log.Logger.Information("Game started!");

                    return startGame.RoomId;
                },
                onAck: (peerId) =>
                {
                    Log.Logger.Information("Game started ack!");
                },
                broadcastMode: BroadcastMode.Room,
                hookId: 1);

            await host
                .RunAsync()
                .ConfigureAwait(false);
        }

        private static IHost BuildHost() =>
            UdpHost
                .CreateHostBuilder()
                .ConfigureHost(settings =>
                {
                    settings.Host = "127.0.0.1";
                    settings.Serializer = new Serializer();
                    settings.LoggerFactory = new SerilogLoggerFactory();
                    settings.InputPorts = new[] { 7000, 7001 };
                    settings.OutputPorts = new[] { 8000, 8001 };
                    settings.Workers = 2;
                    settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                    settings.PeerInactivityTimeout = TimeSpan.FromSeconds(120);
                })
                .ConfigureHostClient((settings) =>
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
