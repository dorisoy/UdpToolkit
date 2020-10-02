namespace UdpToolkit.Integration.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit.Core;
    using UdpToolkit.Framework;
    using UdpToolkit.Integration.Tests.Resources;
    using UdpToolkit.Integration.Tests.Utils;
    using Xunit;

    public class IntegrationTests
    {
        static IntegrationTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();
        }

        [Theory(Timeout = 10_000)]
        [InlineData(UdpMode.Udp)]
        [InlineData(UdpMode.Sequenced)]
        [InlineData(UdpMode.ReliableUdp)]
        public void ClientServer(UdpMode udpMode)
        {
            var expectedPayload = "pong";
            var waitCallBackTimeout = TimeSpan.FromSeconds(10);

            var serverInputPorts = Gen.GenerateUdpPorts(count: 2);

            var serverHost = HostFactory.CreateServerHost(
                inputPorts: serverInputPorts,
                outputPorts: Gen.GenerateUdpPorts(count: 2));

            var clientHost = HostFactory.CreateClientHost(
                inputPorts: Gen.GenerateUdpPorts(count: 2),
                outputPorts: Gen.GenerateUdpPorts(count: 2),
                serverInputPorts: serverInputPorts);

#pragma warning disable 4014
            Task.Run(() => serverHost.RunAsync());
            Task.Run(() => clientHost.RunAsync());
#pragma warning restore 4014

            clientHost.ServerHostClient.Connect();

            var waitCallback = new ManualResetEvent(initialState: false);

            string actualPayload = null;
            serverHost.On<Ping>(
                handler: (peerId, ping) =>
                {
                    actualPayload = "pong";
                    waitCallback.Set();
                },
                hookId: 0);

            Task.Run(() => clientHost.ServerHostClient.Publish(
                @event: new Ping("ping"),
                hookId: 0,
                udpMode: udpMode));

            waitCallback.WaitOne(timeout: waitCallBackTimeout);

            Assert.Equal(expected: expectedPayload, actual: actualPayload);
        }

        [Fact(Timeout = 10_000)]
        public void P2P()
        {
            var expectedPayload = "pong";
            var waitCallback = new ManualResetEvent(initialState: false);
            var timeout = TimeSpan.FromSeconds(15);

            var client2Port = Gen.GenerateUdpPorts(count: 1);

            var destinationIp = new IPEndPoint(IPAddress.Parse("0.0.0.0"), client2Port.Single());

            var client1Host = HostFactory.CreateClientHost(
                inputPorts: Gen.GenerateUdpPorts(1),
                outputPorts: Gen.GenerateUdpPorts(count: 1),
                serverInputPorts: client2Port);

            var client2Host = HostFactory.CreateClientHost(
                inputPorts: client2Port,
                outputPorts: Gen.GenerateUdpPorts(count: 1),
                serverInputPorts: Array.Empty<int>());

#pragma warning disable 4014
            Task.Run(() => client1Host.RunAsync());
            Task.Run(() => client2Host.RunAsync());
#pragma warning restore 4014

            client1Host.ServerHostClient.Connect();

            string actualPayload = null;
            client2Host.On<Ping>(
                handler: (peerId, ping) =>
                {
                    actualPayload = "pong";
                    waitCallback.Set();
                },
                hookId: 0);

            client1Host.ServerHostClient.PublishP2P(
                @event: new Ping("ping"),
                ipEndPoint: destinationIp,
                hookId: 0,
                udpMode: UdpMode.Udp);

            waitCallback.WaitOne(timeout: timeout);

            Assert.Equal(expected: expectedPayload, actual: actualPayload);
        }

        [Fact(Timeout = 10_000)]
        public void ServerDisposed()
        {
            var serverInputPorts = Gen.GenerateUdpPorts(2);
            var serverOutputPorts = Gen.GenerateUdpPorts(2);

            var serverHost = HostFactory.CreateServerHost(
                inputPorts: serverInputPorts,
                outputPorts: serverOutputPorts);

            var host = serverHost;
            Task.Run(() => host.RunAsync());

            serverHost.Stop();
            serverHost.Dispose();
        }
    }
}