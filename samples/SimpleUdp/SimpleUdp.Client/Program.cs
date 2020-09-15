namespace SimpleUdp.Client
{
    using System;
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using SimpleUdp.Contracts;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Framework;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var host = BuildHost();

            host.On<Connected>(
                handler: (peerId, connected) =>
                {
                    Log.Logger.Information($"New peer connected - {peerId}");
                },
                packetType: PacketType.Connected);

            host.On<JoinedEvent>(
                handler: (peerId, joinedEvent) =>
                {
                    Log.Logger.Information($"{joinedEvent.Nickname} joined to room {peerId}!");
                },
                hookId: 1);

#pragma warning disable
            Task.Run(() => host.RunAsync());
#pragma warning restore

            var serverHostClient = host.ServerHostClient;

            serverHostClient.Connect();

            await Task.Delay(5000).ConfigureAwait(false);

            host.Publish(
                datagramFactory: (builder) => builder.ToServer(new JoinEvent(0, "keygen"), 0),
                udpMode: UdpMode.ReliableUdp);

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
