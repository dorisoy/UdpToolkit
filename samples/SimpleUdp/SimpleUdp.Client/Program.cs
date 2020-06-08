namespace SimpleUdp.Client
{
    using System;
    using System.Threading;
    using Serilog;
    using Serilog.Events;
    using SimpleUdp.Contracts;
    using UdpToolkit.Framework.Client.Core;
    using UdpToolkit.Framework.Client.Host;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var clientHost = BuildClientHost();

            clientHost.On<JoinEvent>((@event) => Log.Logger.Information($"Player {@event.Index} joined to room!"));

            new Thread(
                start: () =>
                {
                    while (true)
                    {
                        clientHost.Publish(new JoinEvent());

                        Thread.Sleep(1000);
                    }
                })
                .Start();

            clientHost.RunAsync();

            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }

        private static IClientHost BuildClientHost()
        {
            return Host
                .CreateClientBuilder()
                .Configure(cfg =>
                {
                    cfg.ServerHost = "0.0.0.0";
                    cfg.Serializer = new Serializer();
                    cfg.ServerInputPorts = new[] { 7000, 7001 };
                    cfg.ServerOutputPorts = new[] { 8000, 8001 };
                    cfg.Receivers = 2;
                    cfg.Senders = 2;
                })
                .Build();
        }
    }
}
