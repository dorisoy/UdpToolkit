namespace Sequenced.Server
{
    using System;
    using Sequenced.Contracts;
    using Serializers;
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
            var groupManager = host.ServiceProvider.GroupManager;
            var broadcaster = host.ServiceProvider.Broadcaster;

            host.On<JoinEvent>(
                onEvent: (connectionId, ip, joinEvent) =>
                {
                    groupManager.JoinOrCreate(joinEvent.GroupId, connectionId);

                    Console.WriteLine($"{joinEvent.Nickname} joined to group!");

                    broadcaster.Broadcast(
                        caller: connectionId,
                        groupId: joinEvent.GroupId,
                        @event: joinEvent,
                        channelId: ReliableChannel.Id,
                        broadcastMode: BroadcastMode.GroupExceptCaller);
                });

            host.On<MoveEvent>(
                onEvent: (connectionId, ip, moveEvent) =>
                {
                    Console.WriteLine($"{moveEvent.From} Moved!");

                    broadcaster.Broadcast(
                        caller: connectionId,
                        groupId: moveEvent.GroupId,
                        @event: moveEvent,
                        channelId: SequencedChannel.Id,
                        broadcastMode: BroadcastMode.GroupExceptCaller);
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
                    settings.AllowIncomingConnections = true;
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
