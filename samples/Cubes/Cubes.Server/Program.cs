namespace Cubes.Server
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cubes.Shared.Server;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit.Core;
    using UdpToolkit.Framework;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        private static readonly List<IEventHandler> Handlers = new List<IEventHandler>();

        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var host = BuildHost();

            var joinEventHandler = new JoinEventHandler(
                host: host,
                dispatcher: null);

            var moveEventHandler = new MoveEventHandler(
                host: host,
                dispatcher: null);

            var spawnEventHandler = new SpawnEventHandler(
                host: host,
                dispatcher: null);

            Handlers.AddRange(new IEventHandler[] { joinEventHandler, moveEventHandler, spawnEventHandler });

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
                    settings.InputPorts = new[] { 7000, 7001 };
                    settings.OutputPorts = new[] { 8000, 8001 };
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