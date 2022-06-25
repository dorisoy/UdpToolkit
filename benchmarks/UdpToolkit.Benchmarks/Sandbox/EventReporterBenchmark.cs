namespace UdpToolkit.Benchmarks.Sandbox
{
    using System;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Network.Contracts.Events;
    using UdpToolkit.Network.Contracts.Events.UdpClient;
    using UdpToolkit.Network.Contracts.Sockets;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class EventReporterBenchmark
    {
#pragma warning disable SA1401

        [Params(100, 1000, 20_000)]
        public int Repeats;
#pragma warning restore SA1401

        private readonly string _id = Guid.NewGuid().ToString();

        public IEnumerable<object[]> Reporters()
        {
            yield return new object[] { new ReporterSealedInheritor(), "inheritor" };
            yield return new object[] { new ReporterSealedInterfaceImpl(), "interface" };
        }

        [Benchmark]
        [ArgumentsSource(nameof(Reporters))]
        public void Without_In_Keyword(INetworkEventReporter eventReporter, string name)
        {
            for (int i = 0; i < Repeats; i++)
            {
                var @event = new UserDefinedReceived(_id, 1, new IpV4Address(123, 123));
                eventReporter.Handle(@event);
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(Reporters))]
        public void Inline_Struct(INetworkEventReporter eventReporter, string name)
        {
            for (int i = 0; i < Repeats; i++)
            {
                eventReporter.Handle(new UserDefinedReceived(_id, 1, new IpV4Address(123, 123)));
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(Reporters))]
        public void With_In_Keyword(INetworkEventReporter eventReporter, string name)
        {
            for (int i = 0; i < Repeats; i++)
            {
                var userDefinedReceived = new UserDefinedReceived(_id, 1, new IpV4Address(123, 123));
                eventReporter.Handle(in userDefinedReceived);
            }
        }

        private sealed class ReporterSealedInterfaceImpl : INetworkEventReporter
        {
            public void Handle(in MtuSizeExceeded @event)
            {
            }

            public void Handle(in InvalidHeaderReceived @event)
            {
            }

            public void Handle(in ConnectionRejected @event)
            {
            }

            public void Handle(in ConnectionAccepted @event)
            {
            }

            public void Handle(in ChannelNotFound @event)
            {
            }

            public void Handle(in ExceptionThrown @event)
            {
            }

            public void Handle(in ConnectionNotFound @event)
            {
            }

            public void Handle(in ReceivingStarted @event)
            {
            }

            public void Handle(in ScanInactiveConnectionsStarted @event)
            {
            }

            public void Handle(in ConnectionRemovedByTimeout @event)
            {
            }

            public void Handle(in PingReceived @event)
            {
            }

            public void Handle(in PingAckReceived @event)
            {
            }

            public void Handle(in DisconnectReceived @event)
            {
            }

            public void Handle(in DisconnectAckReceived @event)
            {
            }

            public void Handle(in ConnectReceived @event)
            {
            }

            public void Handle(in ConnectAckReceived @event)
            {
            }

            public void Handle(in UserDefinedReceived @event)
            {
            }

            public void Handle(in UserDefinedAckReceived @event)
            {
            }

            public void Handle(in PendingPacketResent @event)
            {
            }

            public void Handle(in ExpiredPacketRemoved @event)
            {
            }
        }

        private sealed class ReporterSealedInheritor : NetworkEventReporter
        {
            public override void Handle(in UserDefinedReceived @event)
            {
            }
        }
    }
}