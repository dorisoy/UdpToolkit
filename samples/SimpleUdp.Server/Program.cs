namespace SimpleUdp.Server
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit.Core;
    using UdpToolkit.Di.AutofacIntegration;
    using UdpToolkit.Framework.Hosts;
    using UdpToolkit.Framework.Pipelines;
    using UdpToolkit.Serialization.MsgPack;
    using UdpToolkit.Utils;

    public static class Program
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var server = BuildServer();

            await server.RunAsync().ConfigureAwait(false);
        }

        private static IServerHost BuildServer() =>
            Host
                .CreateServerBuilder(new ContainerBuilder())
                .Configure(settings =>
                {
                    settings.ServerHost = "0.0.0.0";
                    settings.InputPorts = new[] { 7000, 7001 };
                    settings.OutputPorts = new[] { 8000, 8001 };
                    settings.ProcessWorkers = 2;
                    settings.Serializer = new Serializer();
                    settings.CacheOptions = new CacheOptions(
                        scanForExpirationFrequency: TimeSpan.FromMinutes(1),
                        cacheEntryTtl: Timeout.InfiniteTimeSpan);
                })
                .Use(pipeline =>
                {
                    pipeline
                        .Append<GlobalScopeStage>()
                        .Append<ProcessStage>();
                })
                .ConfigureServices(builder =>
                {
                    builder.RegisterSingleton<IService, Service>(() => new Service());
                })
                .Build();
    }
}
