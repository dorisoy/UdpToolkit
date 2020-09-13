namespace UdpToolkit.Integration.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Framework;
    using UdpToolkit.Integration.Tests.Utils;
    using UdpToolkit.Network.Channels;
    using Xunit;
    using Xunit.Abstractions;

    public class ProtocolTests
    {
        [Fact(Timeout = 10_000)]
        public void Connected()
        {
            var serverInputPorts = Gen.GenerateUdpPorts(count: 2);
            var waitCallBackTimeout = TimeSpan.FromSeconds(10);

            var serverHost = HostFactory.CreateServerHost(
                inputPorts: serverInputPorts,
                outputPorts: Gen.GenerateUdpPorts(count: 2));

            var clientHost = HostFactory.CreateClientHost(
                inputPorts: Gen.GenerateUdpPorts(count: 2),
                outputPorts: Gen.GenerateUdpPorts(count: 2),
                serverInputPorts: serverInputPorts);

            var waitCallback = new ManualResetEvent(initialState: false);

            Guid? receivedPeerId = null;
            serverHost.OnProtocolInternal<Connect, Connected>(
                handler: (peerId, connect, builder) =>
                {
                    Log.Logger.Information($"Connected with id - {peerId}");

                    return builder.Caller(new Connected(peerId), peerId, (byte)PacketType.Connected);
                },
                hookId: (byte)PacketType.Connect);

            clientHost.OnProtocolInternal<Connected>(
                handler: (guid, @event) =>
                {
                    receivedPeerId = guid;
                    waitCallback.Set();
                },
                hookId: (byte)PacketType.Connected);

#pragma warning disable 4014
            Task.Run(() => serverHost.RunAsync());
            Task.Run(() => clientHost.RunAsync());
#pragma warning restore 4014

            var client = clientHost.ServerHostClient;
            client.Connect();

            waitCallback.WaitOne(timeout: waitCallBackTimeout);

            Assert.True(receivedPeerId.HasValue);
            Assert.NotEqual(Guid.Empty, receivedPeerId.Value);
        }

        [Fact(Timeout = 10_000)]
        public void Disconnected()
        {
            var serverInputPorts = Gen.GenerateUdpPorts(count: 2);
            var waitCallBackTimeout = TimeSpan.FromSeconds(10);

            var serverHost = HostFactory.CreateServerHost(
                inputPorts: serverInputPorts,
                outputPorts: Gen.GenerateUdpPorts(count: 2));

            var clientHost = HostFactory.CreateClientHost(
                inputPorts: Gen.GenerateUdpPorts(count: 2),
                outputPorts: Gen.GenerateUdpPorts(count: 2),
                serverInputPorts: serverInputPorts);

            var waitCallback1 = new ManualResetEvent(initialState: false);
            var waitCallback2 = new ManualResetEvent(initialState: false);

            Guid? connectedId = null;
            serverHost.OnProtocolInternal<Connect, Connected>(
                handler: (peerId, connect, builder) =>
                {
                    Log.Logger.Information($"Connected with id - {peerId}");

                    return builder.Caller(new Connected(peerId), peerId, (byte)PacketType.Connected);
                },
                hookId: (byte)PacketType.Connect);

            clientHost.OnProtocolInternal<Connected>(
                handler: (guid, @event) =>
                {
                    connectedId = guid;
                    waitCallback1.Set();
                },
                hookId: (byte)PacketType.Connected);

#pragma warning disable 4014
            Task.Run(() => serverHost.RunAsync());
            Task.Run(() => clientHost.RunAsync());
#pragma warning restore 4014

            var client = clientHost.ServerHostClient;
            client.Connect();

            waitCallback1.WaitOne(timeout: waitCallBackTimeout);

            serverHost.OnProtocolInternal<Disconnect, Disconnected>(
                handler: (peerId, connect, builder) =>
                {
                    Log.Logger.Information($"Connected with id - {peerId}");

                    return builder.Caller(new Disconnected(peerId), peerId, (byte)PacketType.Disconnected);
                },
                hookId: (byte)PacketType.Disconnect);

            Guid? disconnectedId = null;
            clientHost.OnProtocolInternal<Disconnected>(
                handler: (guid, @event) =>
                {
                    disconnectedId = guid;
                    waitCallback2.Set();
                },
                hookId: (byte)PacketType.Disconnected);

            client.Disconnect();
            waitCallback2.WaitOne(timeout: waitCallBackTimeout);

            Assert.True(connectedId.HasValue && disconnectedId.HasValue);
            Assert.Equal(connectedId.HasValue, disconnectedId.HasValue);
        }
    }
}