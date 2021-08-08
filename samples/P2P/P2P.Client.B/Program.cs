namespace P2P.Client.B
{
    using System;
    using System.Threading;
    using P2P.Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Executors;
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        private static readonly string Host = "127.0.0.1";
        private static readonly int Port = 3000;
        private static bool _connectedToPeer = false;

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            var nickname = "ClientB";
            var host = BuildHost();
            var client = host.HostClient;

            host.OnProtocol<Connect>(
                onProtocolEvent: (connectionId, connected) =>
                {
                    Log.Logger.Information($"Must be raised only on server side");
                },
                onAck: (connectionId) =>
                {
                    Log.Logger.Information($"{nickname} connected with connectionId - {connectionId}");
                },
                onTimeout: (connectionId) =>
                {
                    Log.Logger.Information($"Connection timeout - {connectionId}");
                },
                protocolHookId: ProtocolHookId.Connect);

            host.OnProtocol<ConnectToPeer>(
                onProtocolEvent: (connectionId, connected) =>
                {
                    Log.Logger.Information($"Incoming peer connection from {connectionId}");
                },
                onAck: (connectionId) =>
                {
                    _connectedToPeer = true;
                },
                onTimeout: (connectionId) =>
                {
                    Log.Logger.Information($"Connection timeout - {connectionId}");
                },
                protocolHookId: ProtocolHookId.Connect2Peer);

            host.On<JoinEvent>(
                onEvent: (connectionId, joinEvent) =>
                {
                    Log.Logger.Information($"{joinEvent.Nickname} joined to room! (event)");
                    return joinEvent.RoomId;
                },
                onAck: (connectionId) =>
                {
                    Log.Logger.Information($"{nickname} joined to room! (ack)");
                },
                broadcastMode: BroadcastMode.RoomExceptCaller,
                hookId: 0);

            host.On<Message>(
                onEvent: (connectionId, message) =>
                {
                    Log.Logger.Information($"P2P message received - {message.Text}! (event)");
                    return message.RoomId;
                },
                onAck: (connectionId) =>
                {
                    Log.Logger.Information($"{nickname} p2p message! (ack)");
                },
                broadcastMode: BroadcastMode.None,
                hookId: 1);

            host.Run();
            client.Connect();

            var waitTimeout = TimeSpan.FromSeconds(120);
            SpinWait.SpinUntil(() => client.IsConnected, waitTimeout);

            client.Send(
                @event: new JoinEvent(roomId: 11, nickname: nickname),
                hookId: 0,
                channelId: ReliableChannel.Id);

            client.ConnectToPeer(host: Host, port: Port);

            SpinWait.SpinUntil(() => _connectedToPeer, waitTimeout);

            int counter = 0;
            while (counter < 1000)
            {
                client.Send(
                    @event: new Message(text: $"p2p message from {nickname}", roomId: 11),
                    hookId: 1,
                    destination: new IpV4Address
                    {
                        Address = Host.ToInt(),
                        Port = (ushort)Port,
                    },
                    channelId: ReliableChannel.Id);

                Thread.Sleep(1000);
                counter++;
            }

            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost((settings) =>
                {
                    settings.Host = "127.0.0.1";
                    settings.Serializer = new Serializer();
                    settings.LoggerFactory = new SerilogLoggerFactory();
                    settings.HostPorts = new[] { 5000, 5001 };
                    settings.Workers = 8;
                    settings.Executor = new ThreadBasedExecutor();
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ServerHost = "127.0.0.1";
                    settings.ServerPorts = new[] { 7000, 7001 };
                    settings.HeartbeatDelayInMs = null; // pass null for disable heartbeat
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.ChannelsFactory = new ChannelsFactory();
                    settings.SocketFactory = new NativeSocketFactory();
                    settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                    settings.ResendTimeout = TimeSpan.FromSeconds(120);
                })
                .Build();
        }
    }
}
