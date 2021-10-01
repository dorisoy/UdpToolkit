namespace UdpToolkit.Network.Tests.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;
    using UdpToolkit.Network.Tests.Framework;
    using Xunit;

    public class UdpClientTests
    {
        public static IEnumerable<object[]> SocketFactories()
        {
            yield return new object[] { new NativeSocketFactory() };
            yield return new object[] { new ManagedSocketFactory() };
        }

        public static IEnumerable<object[]> PacketsWithInvalidHeader()
        {
            for (int i = 1; i < Utils.GetNetworkHeaderSize(); i++)
            {
                yield return new object[] { Gen.GenerateRandomBytes(i), new NativeSocketFactory() };
            }
        }

        [Theory]
        [MemberData(nameof(SocketFactories))]
        public async Task UdpClientConnectedAsync(
            ISocketFactory socketFactory)
        {
            var host = "127.0.0.1";
            var clientPort = Utils.GetAvailablePort();
            var serverPort = Utils.GetAvailablePort();
            var buffer = new List<ConnectionInfo>();

            using var client = Utils.CreateUdpClient(host, (ushort)clientPort, socketFactory);
            using var server = Utils.CreateUdpClient(host, (ushort)serverPort, socketFactory);

            var serverIp = server.GetLocalIp();

            var connectionId = await client
                .ConnectAsync(serverIp, buffer)
                .ConfigureAwait(false);

            var expected = new
            {
                ConnectionId = connectionId,
                Ip = serverIp,
            };

            buffer
                .Should()
                .ContainSingle();

            buffer
                .Single()
                .Should()
                .BeEquivalentTo(expected);

            client
                .IsConnected(out var connId)
                .Should()
                .BeTrue();

            connId
                .Should()
                .Be(connectionId);
        }

        [Theory]
        [MemberData(nameof(SocketFactories))]
        public async Task UdpClientDisconnectedAsync(
            ISocketFactory socketFactory)
        {
            var host = "127.0.0.1";
            var clientPort = Utils.GetAvailablePort();
            var serverPort = Utils.GetAvailablePort();
            var buffer = new List<ConnectionInfo>();

            using var client = Utils.CreateUdpClient(host, (ushort)clientPort, socketFactory);
            using var server = Utils.CreateUdpClient(host, (ushort)serverPort, socketFactory);

            var serverIp = server.GetLocalIp();

            var connectionId = await client
                .ConnectAsync(serverIp, new List<ConnectionInfo>())
                .ConfigureAwait(false);

            await client
                .DisconnectAsync(serverIp, buffer)
                .ConfigureAwait(false);

            var expected = new
            {
                ConnectionId = connectionId,
                Ip = serverIp,
            };

            buffer
                .Should()
                .ContainSingle();

            buffer
                .Single()
                .Should()
                .BeEquivalentTo(expected);

            client
                .IsConnected(out var connId)
                .Should()
                .BeFalse();

            connId
                .Should()
                .Be(Guid.Empty);
        }

        [Theory]
        [MemberData(nameof(SocketFactories))]
        public async Task HeartbeatReceivedAsync(
            ISocketFactory socketFactory)
        {
            var host = "127.0.0.1";
            var clientPort = Utils.GetAvailablePort();
            var serverPort = Utils.GetAvailablePort();
            var buffer = new List<HeartbeatInfo>();

            using var client = Utils.CreateUdpClient(host, (ushort)clientPort, socketFactory);
            using var server = Utils.CreateUdpClient(host, (ushort)serverPort, socketFactory);

            var serverIp = server.GetLocalIp();

            var connectionId = await client
                .ConnectAsync(serverIp, new List<ConnectionInfo>())
                .ConfigureAwait(false);

            await client
                .HeartbeatAsync(serverIp, buffer)
                .ConfigureAwait(false);

            var expected = new
            {
                ConnectionId = connectionId,
                Rtt = TimeSpan.FromSeconds(1),
            };

            buffer
                .Should()
                .ContainSingle();

            buffer
                .Single()
                .Should()
                .BeEquivalentTo(expected, options => options.Excluding(exp => exp.Rtt));
        }

        [Theory]
        [MemberData(nameof(SocketFactories))]
        public async Task HeartbeatNotSentWhenClientNotConnectedAsync(
            ISocketFactory socketFactory)
        {
            var host = "127.0.0.1";
            var clientPort = Utils.GetAvailablePort();
            var serverPort = Utils.GetAvailablePort();
            var buffer = new List<HeartbeatInfo>();

            using var client = Utils.CreateUdpClient(host, (ushort)clientPort, socketFactory);
            var serverIp = new IpV4Address(host.ToInt(), (ushort)serverPort);

            await client
                .HeartbeatAsync(serverIp, buffer)
                .ConfigureAwait(false);

            buffer
                .Should()
                .BeEmpty();
        }

        [Theory]
        [MemberData(nameof(SocketFactories))]
        public async Task SentPacketReceivedAsync(
            ISocketFactory socketFactory)
        {
            var host = "127.0.0.1";
            var clientPort = Utils.GetAvailablePort();
            var serverPort = Utils.GetAvailablePort();
            var buffer = new List<PacketInfo>();

            using var client = Utils.CreateUdpClient(host, (ushort)clientPort, socketFactory);
            using var server = Utils.CreateUdpClient(host, (ushort)serverPort, socketFactory);

            var payload = Gen.GenerateRandomBytes(100);
            var serverIp = server.GetLocalIp();

            var connectionId = await client
                .ConnectAsync(serverIp, new List<ConnectionInfo>())
                .ConfigureAwait(false);

            var packetInfoTask = server.WaitNewPacketsAsync(buffer);
            client.Send(connectionId, ReliableChannel.Id, 1, payload, serverIp);
            await packetInfoTask.ConfigureAwait(false);

            var expected = new
            {
                ConnectionId = connectionId,
                Ip = new IpV4Address(host.ToInt(), (ushort)clientPort),
                DataType = 1,
                ChannelId = ReliableChannel.Id,
                Payload = payload,
            };

            buffer
                .Should()
                .ContainSingle();

            buffer
                .Single()
                .Should()
                .BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(SocketFactories))]
        public async Task PacketsWithMtuSizeLimitExceededDroppedAsync(
            ISocketFactory socketFactory)
        {
            var host = "127.0.0.1";
            var clientPort = Utils.GetAvailablePort();
            var serverPort = Utils.GetAvailablePort();
            var buffer = new List<PacketInfo>();
            var mtuSize = Gen.RandomInt();

            using var client = Utils.CreateUdpClient(host, (ushort)clientPort, socketFactory, mtuSize);
            using var server = Utils.CreateUdpClient(host, (ushort)serverPort, socketFactory, mtuSize);

            var payload = Gen.GenerateRandomBytes(mtuSize);
            var serverIp = server.GetLocalIp();

            var connectionId = await client
                .ConnectAsync(serverIp, new List<ConnectionInfo>())
                .ConfigureAwait(false);

            var packetInfoTask = client.WaitDroppedPacketAsync(buffer);
            client.Send(connectionId, ReliableChannel.Id, 1, payload, serverIp);
            await packetInfoTask.ConfigureAwait(false);

            var expected = new
            {
                ConnectionId = connectionId,
                Ip = new IpV4Address(host.ToInt(), (ushort)serverPort),
                DataType = 1,
                ChannelId = ReliableChannel.Id,
                Payload = payload,
            };

            buffer
                .Should()
                .ContainSingle();

            buffer
                .Single()
                .Should()
                .BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(SocketFactories))]
        public async Task DisconnectNotSentWhenClientNotConnectedAsync(
            ISocketFactory socketFactory)
        {
            var host = "127.0.0.1";
            var clientPort = Utils.GetAvailablePort();
            var serverPort = Utils.GetAvailablePort();
            var buffer = new List<ConnectionInfo>();

            using var client = Utils.CreateUdpClient(host, (ushort)clientPort, socketFactory);
            var serverIp = new IpV4Address(host.ToInt(), (ushort)serverPort);

            await client
                .DisconnectAsync(serverIp, buffer)
                .ConfigureAwait(false);

            buffer
                .Should()
                .BeEmpty();
        }

        [Theory]
        [MemberData(nameof(SocketFactories))]
        public void DoubleDisposeNotThrownException(
            ISocketFactory socketFactory)
        {
            var client = Utils.CreateUdpClient("0", 0, socketFactory);
            Action doubleDispose = () =>
            {
                client.Dispose();
                client.Dispose();
            };

            doubleDispose
                .Invoking(a => a())
                .Should()
                .NotThrow();
        }

        [Theory]
        [MemberData(nameof(PacketsWithInvalidHeader))]
        public async Task PacketsWithLengthLessThanNetworkHeaderDropped(
            byte[] payload,
            ISocketFactory socketFactory)
        {
            var host = "127.0.0.1";
            var serverPort = Utils.GetAvailablePort();
            var clientPort = Utils.GetAvailablePort();

            var brokenClient = new UdpClient(clientPort);
            using var server = Utils.CreateUdpClient(host, (ushort)serverPort, socketFactory);

            var buffer = new List<InvalidPacketInfo>();

            var invalidPacketsTask = server.WaitInvalidPacketsAsync(buffer);

            await brokenClient
                .SendAsync(payload, payload.Length, host, serverPort)
                .ConfigureAwait(false);

            await invalidPacketsTask.ConfigureAwait(false);

            var expected = new
            {
                Ip = new IpV4Address(host.ToInt(), (ushort)clientPort),
                Payload = Utils.Extend(payload, 2048),
            };

            buffer
                .Should()
                .ContainSingle();

            buffer
                .Single()
                .Should()
                .BeEquivalentTo(expected);
        }
    }
}