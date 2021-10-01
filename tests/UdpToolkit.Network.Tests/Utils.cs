namespace UdpToolkit.Network.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Tests.Framework;
    using UdpToolkit.Network.Utils;

    internal static class Utils
    {
        private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);

        internal static IUdpClient CreateUdpClient(
            string host,
            ushort port,
            ISocketFactory socketFactory,
            int mtuSize = int.MaxValue)
        {
            var factory = new UdpClientFactory(
                udpClientSettings: new UdpClientSettings(
                    mtuSizeLimit: mtuSize,
                    udpClientBufferSize: 2048,
                    pollFrequency: 15,
                    allowIncomingConnections: true,
                    resendTimeout: TimeSpan.FromSeconds(10),
                    channelsFactory: new ChannelsFactory(),
                    socketFactory: socketFactory),
                connectionPoolSettings: new ConnectionPoolSettings(
                    connectionTimeout: TimeSpan.FromSeconds(10),
                    connectionsCleanupFrequency: TimeSpan.FromSeconds(10)),
                loggerFactory: new SimpleConsoleLoggerFactory(LogLevel.Debug),
                dateTimeProvider: new DateTimeProvider());

            var client = factory.Create(new IpV4Address(address: host.ToInt(), port: port));

            Task.Run(() => client.StartReceive(default));

            return client;
        }

        internal static async Task<Guid> ConnectAsync(
            this IUdpClient udpClient,
            IpV4Address serverIp,
            List<ConnectionInfo> buffer,
            int delayInSeconds = 5)
        {
            var connectionId = Guid.NewGuid();
            var connectionTask = SubscribeOnConnected();
            udpClient.Connect(serverIp, connectionId);
            await connectionTask.ConfigureAwait(false);
            return connectionId;

            Task SubscribeOnConnected()
            {
                var signal = new SemaphoreSlim(0, 1);
                udpClient.OnConnected += (ip, connId) =>
                {
                    buffer.Add(new ConnectionInfo(ip, connId));
                    signal.Release();
                };

                return signal.WaitAsync(TimeSpan.FromSeconds(delayInSeconds));
            }
        }

        internal static Task DisconnectAsync(
            this IUdpClient udpClient,
            IpV4Address serverIp,
            List<ConnectionInfo> buffer,
            int delayInSeconds = 5)
        {
            var disconnectionTask = SubscribeOnDisconnected();
            udpClient.Disconnect(serverIp);
            return disconnectionTask;

            Task SubscribeOnDisconnected()
            {
                var signal = new SemaphoreSlim(0, 1);
                udpClient.OnDisconnected += (ipAddress, connId) =>
                {
                    buffer.Add(new ConnectionInfo(ipAddress, connId));
                    signal.Release();
                };

                return signal.WaitAsync(TimeSpan.FromSeconds(delayInSeconds));
            }
        }

        internal static Task HeartbeatAsync(
            this IUdpClient udpClient,
            IpV4Address serverIp,
            List<HeartbeatInfo> buffer)
        {
            var heartbeatTask = SubscribeOnHeartbeat();
            udpClient.Heartbeat(serverIp);
            return heartbeatTask;

            Task SubscribeOnHeartbeat()
            {
                var signal = new SemaphoreSlim(0, 1);
                udpClient.OnHeartbeat += (connectionId, rtt) =>
                {
                    buffer.Add(new HeartbeatInfo(connectionId, rtt));
                    signal.Release();
                };

                return signal.WaitAsync(TimeSpan.FromSeconds(5));
            }
        }

        internal static Task WaitNewPacketsAsync(
            this IUdpClient udpClient,
            List<PacketInfo> buffer)
        {
            var signal = new SemaphoreSlim(0, 1);
            udpClient.OnPacketReceived += (ip, connectionId, payload, channelId, dataType) =>
            {
                buffer.Add(new PacketInfo(connectionId, ip, dataType, channelId, payload));
                signal.Release();
            };

            return signal.WaitAsync(TimeSpan.FromSeconds(5));
        }

        internal static Task WaitDroppedPacketAsync(
            this IUdpClient udpClient,
            List<PacketInfo> buffer)
        {
            var signal = new SemaphoreSlim(0, 1);
            udpClient.OnPacketDropped += (ip, connectionId, payload, channelId, dataType) =>
            {
                buffer.Add(new PacketInfo(connectionId, ip, dataType, channelId, payload));
                signal.Release();
            };

            return signal.WaitAsync(TimeSpan.FromSeconds(5));
        }

        internal static Task WaitInvalidPacketsAsync(
            this IUdpClient udpClient,
            List<InvalidPacketInfo> buffer)
        {
            var signal = new SemaphoreSlim(0, 1);
            udpClient.OnInvalidPacketReceived += (ip, payload) =>
            {
                buffer.Add(new InvalidPacketInfo(ip, payload));
                signal.Release();
            };

            return signal.WaitAsync(TimeSpan.FromSeconds(5));
        }

        internal static byte[] Extend(byte[] bytes, int size)
        {
            var result = new byte[size];
            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = bytes[i];
            }

            return result;
        }

        internal static int GetAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(DefaultLoopbackEndpoint);
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        internal static unsafe int GetNetworkHeaderSize() => sizeof(NetworkHeader);
    }
}