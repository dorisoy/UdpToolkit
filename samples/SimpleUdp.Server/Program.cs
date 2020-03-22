namespace SimpleUdp.Server
{
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Hosts;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();

            var server = BuildServer();

            await server.RunAsync().ConfigureAwait(false);
        }

        private static IServerHost BuildServer() =>
            Host
                .CreateServerBuilder()
                .Configure(settings =>
                {
                    settings.ServerHost = "0.0.0.0";
                    settings.InputPorts = new[] { 7000, 7001 };
                    settings.OutputPorts = new[] { 8000, 8001 };
                    settings.ProcessWorkers = 2;
                    settings.Serializer = new Serializer();
                })
                .ConfigureServices(builder =>
                {
                    builder.RegisterSingleton<IService, Service>(() => new Service());
                })
                .Build();
    }
}
