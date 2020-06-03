namespace SimpleUdp.Client
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using SimpleUdp.Contracts;
    using UdpToolkit.Framework.Client.Core;
    using UdpToolkit.Framework.Client.Host;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        private static readonly byte RoomId = 0;
        private static readonly string Nickname = "Foo";

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();

            var clientHost = BuildClientHost();

            var consumerFactory = clientHost.GetEventConsumerFactory();
            var producerFactory = clientHost.GetEventProducerFactory();

            var joinedConsumer = consumerFactory.Create<JoinedEvent>();
            var leavedConsumer = consumerFactory.Create<LeavedEvent>();

            var joinProducer = producerFactory.Create<JoinEvent>(roomId: 0);
            var leaveProducer = producerFactory.Create<LeaveEvent>(roomId: 0);

            new Thread(() => Consume(joinedConsumer)).Start();
            new Thread(() => Produce(joinProducer)).Start();

            await clientHost
                .RunAsync()
                .ConfigureAwait(false);
        }

        private static void Produce(IEventProducer<JoinEvent> producer)
        {
            while (true)
            {
                producer.Produce(new JoinEvent
                {
                    RoomId = RoomId,
                    Nickname = Nickname,
                });

                Thread.Sleep(1000);
            }
        }

        private static void Consume(IEventConsumer<JoinedEvent> consumer)
        {
            while (true)
            {
                var events = consumer.Consume();
                foreach (var @event in events)
                {
                    Log.Logger.Information($"Player {@event.Nickname} joined to room!");
                }

                Thread.Sleep(1000);
            }
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
