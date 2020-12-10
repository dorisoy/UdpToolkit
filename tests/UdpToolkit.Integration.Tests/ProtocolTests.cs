namespace UdpToolkit.Integration.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Framework;
    using UdpToolkit.Integration.Tests.Utils;
    using UdpToolkit.Network.Channels;
    using Xunit;

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

            clientHost.OnProtocol<Connect>(
                onProtocolEvent: (guid, @event) =>
                {
                    receivedPeerId = guid;
                    waitCallback.Set();
                },
                onAck: (id) => { },
                onTimeout: (id) => { },
                protocolHookId: ProtocolHookId.Connect);

#pragma warning disable 4014
            Task.Run(() => serverHost.RunAsync());
            Task.Run(() => clientHost.RunAsync());
#pragma warning restore 4014

            var client = clientHost.ServerHostClient;
            client.Connect(connectionTimeout: TimeSpan.FromSeconds(5));

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
            clientHost.OnProtocol<Connect>(
                onProtocolEvent: (guid, @event) =>
                {
                    connectedId = guid;
                    waitCallback1.Set();
                },
                onAck: (id) => { },
                onTimeout: (id) => { },
                protocolHookId: ProtocolHookId.Connect);

            Guid? disconnectedId = null;
            clientHost.OnProtocol<Disconnect>(
                onProtocolEvent: (guid, @event) =>
                {
                    disconnectedId = guid;
                    waitCallback2.Set();
                },
                onAck: (id) => { },
                onTimeout: (id) => { },
                protocolHookId: ProtocolHookId.Disconnect);

#pragma warning disable 4014
            Task.Run(() => serverHost.RunAsync());
            Task.Run(() => clientHost.RunAsync());
#pragma warning restore 4014

            var client = clientHost.ServerHostClient;
            client.Connect(connectionTimeout: TimeSpan.FromSeconds(5));

            waitCallback1.WaitOne(timeout: waitCallBackTimeout);

            client.Disconnect();
            waitCallback2.WaitOne(timeout: waitCallBackTimeout);

            Assert.True(connectedId.HasValue && disconnectedId.HasValue);
            Assert.Equal(connectedId.HasValue, disconnectedId.HasValue);
        }
    }
}