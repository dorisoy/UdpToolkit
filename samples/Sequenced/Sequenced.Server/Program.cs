namespace Sequenced.Server
{
    using System;
    using Sequenced.Contracts;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public static class Program
    {
        public static void Main()
        {
            var host = BuildHost();
            var roomManager = host.ServiceProvider.RoomManager;
            var broadcaster = host.ServiceProvider.Broadcaster;

            host.On<JoinEvent>(
                onEvent: (connectionId, ip, joinEvent) =>
                {
                    roomManager.JoinOrCreate(joinEvent.RoomId, connectionId, ip);

                    Console.WriteLine($"{joinEvent.Nickname} joined to room!");

                    broadcaster.Broadcast(
                        caller: connectionId,
                        roomId: joinEvent.RoomId,
                        @event: joinEvent,
                        channelId: ReliableChannel.Id,
                        broadcastMode: BroadcastMode.RoomExceptCaller);
                });

            host.On<MoveEvent>(
                onEvent: (connectionId, ip, moveEvent) =>
                {
                    Console.WriteLine($"{moveEvent.From} Moved!");

                    broadcaster.Broadcast(
                        caller: connectionId,
                        roomId: moveEvent.RoomId,
                        @event: moveEvent,
                        channelId: SequencedChannel.Id,
                        broadcastMode: BroadcastMode.RoomExceptCaller);
                });

            host.Run();

            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new MessagePackSerializer());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, settings =>
                {
                    settings.Host = "127.0.0.1";
                    settings.HostPorts = new[] { 7000, 7001 };
                    settings.Workers = 8;
                    settings.Executor = new ThreadBasedExecutor();
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ResendTimeout = TimeSpan.FromSeconds(120);
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new NativeSocketFactory();
                    settings.ConnectionIdFactory = new ConnectionIdFactory();
                    settings.AllowIncomingConnections = true;
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
