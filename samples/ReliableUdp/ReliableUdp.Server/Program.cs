namespace ReliableUdp.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ReliableUdp.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Core;
    using UdpToolkit.Core.Executors;
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

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var host = BuildHost();

            host.On<JoinEvent, StartGame>(
                onEvent: (connectionId, joinEvent, roomManager) =>
                {
                    roomManager
                        .JoinOrCreate(joinEvent.RoomId, connectionId);

                    Log.Logger.Information($"{joinEvent.Nickname} joined to room!");

                    var peers = roomManager
                        .GetRoom(joinEvent.RoomId)
                        .RoomConnections
                        .Select(x => x.ConnectionId);

                    var spawnPositions = peers
                        .Select(id => new { id, position = Positions.Dequeue() })
                        .ToDictionary(pair => pair.id, pair => pair.position);

                    return (
                        roomId: joinEvent.RoomId,
                        response: new StartGame(joinEvent.RoomId, spawnPositions),
                        delayInMs: 20_000,
                        broadcastMode: BroadcastMode.Room,
                        uppMode: UdpMode.ReliableUdp);
                },
                broadcastMode: BroadcastMode.None,
                hookId: 0);

            host.On<StartGame>(
                onEvent: (connectionId, startGame) =>
                {
                    Log.Logger.Information("Game started!");

                    return startGame.RoomId;
                },
                onAck: (connectionId) =>
                {
                    Log.Logger.Information("Game started ack!");
                },
                broadcastMode: BroadcastMode.Room,
                hookId: 1);

            host.Run();

            Console.ReadLine();
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
                    settings.ConnectionTtl = TimeSpan.FromSeconds(30);
                    settings.ExecutorType = ExecutorType.ThreadBasedExecutor;
                })
                .Build();
    }
}
