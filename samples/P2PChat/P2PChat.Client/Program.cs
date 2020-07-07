namespace P2PChat.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using P2PChat.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit.Core;
    using UdpToolkit.Framework;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        private static ConcurrentDictionary<string, int> _peers = new ConcurrentDictionary<string, int>();

        public static void Main(string[] args)
        {
            var nickname = args[0];
            var messageText = args[1];
            var sleep = int.Parse(args[2]);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var clientHost = BuildClientHost();
            var hostClient = CreateHostClient();

            Task.Run(() => clientHost.RunAsync());

            Console.ReadLine();
        }

        private static IServerHostClient CreateHostClient()
        {
            throw new NotImplementedException();
        }

        private static IHost BuildClientHost()
        {
            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(cfg =>
                {
                    cfg.Host = "0.0.0.0";
                    cfg.Serializer = new Serializer();
                    cfg.InputPorts = new[] { 7000, 7001 };
                    cfg.OutputPorts = new[] { 8000, 8001 };
                    cfg.Receivers = 2;
                    cfg.Senders = 2;
                })
                .Build();
        }
    }
}