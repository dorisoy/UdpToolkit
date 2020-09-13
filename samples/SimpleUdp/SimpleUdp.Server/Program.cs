namespace SimpleUdp.Server
{
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit.Core;
    using UdpToolkit.Framework;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var serverHost = BuildHost();

            await serverHost
                .RunAsync()
                .ConfigureAwait(false);
        }

        private static IHost BuildHost() =>
            UdpHost
                .CreateHostBuilder()
                .ConfigureHost(settings =>
                {
                    settings.Host = "0.0.0.0";
                    settings.InputPorts = new[] { 7000, 7001 };
                    settings.OutputPorts = new[] { 8000, 8001 };
                    settings.Workers = 2;
                    settings.Serializer = new Serializer();
                })
                .Build();
    }
}
