namespace SimpleUdp.Client
{
    using System;
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit.Core;
    using UdpToolkit.Framework;
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

#pragma warning disable
            Task.Run(() => host.RunAsync());
#pragma warning restore

            var serverHostClient = host.ServerHostClient;

            serverHostClient.Connect(TimeSpan.FromSeconds(5));

#pragma warning disable
            // serverHostClient.Disconnect();
#pragma warning restore

            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost((settings) =>
                {
                    settings.Host = "0.0.0.0";
                    settings.Serializer = new Serializer();
                    settings.InputPorts = new[] { 5000, 5001 };
                    settings.OutputPorts = new[] { 6000, 6001 };
                    settings.Workers = 2;
                })
                .ConfigureServerHostClient((settings) =>
                {
                    settings.ServerHost = "0.0.0.0";
                    settings.ServerPorts = new[] { 7000, 7001 };
                })
                .Build();
        }
    }
}
