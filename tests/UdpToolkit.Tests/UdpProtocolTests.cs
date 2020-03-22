namespace UdpToolkit.Tests
{
    using System.Linq;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Tests.Utils;
    using Xunit;

    public class UdpProtocolTests
    {
        [Fact]
        public void UdpPacket_Created()
        {
            var protocol = CreateUdpProtocol();

            var payload = Enumerable
                .Range(0, Gen.GetRandomUshort(10, 100))
                .Select(_ => Gen.GetRandomByte())
                .ToArray();

            var fh = new FrameworkHeader(
                hubId: Gen.GetRandomByte(),
                rpcId: Gen.GetRandomByte(),
                scopeId: Gen.GetRandomUshort());

            var bytes = protocol.GetUdpPacketBytes(frameworkHeader: fh, payload: payload);

            Assert.Equal(
                expected: Consts.FrameworkHeaderLength + Consts.PacketTypeHeaderLength + payload.Length,
                actual: bytes.Length);
        }

        [Fact]
        public void ReliableUdpPacket_Created()
        {
            var protocol = CreateUdpProtocol();

            var payload = Enumerable
                .Range(0, Gen.GetRandomUshort(10, 100))
                .Select(_ => Gen.GetRandomByte())
                .ToArray();

            var fh = new FrameworkHeader(
                hubId: Gen.GetRandomByte(),
                rpcId: Gen.GetRandomByte(),
                scopeId: Gen.GetRandomUshort());

            var rh = new ReliableUdpHeader(
                localNumber: Gen.GetRandomUint(),
                ack: Gen.GetRandomUint(),
                acks: Gen.GetRandomUint());

            var bytes = protocol.GetReliableUdpPacketBytes(
                frameworkHeader: fh,
                reliableUdpHeader: rh,
                payload: payload);

            Assert.Equal(
                expected: Consts.FrameworkHeaderLength + Consts.PacketTypeHeaderLength + Consts.ReliableUdpProtocolHeaderLength + payload.Length,
                actual: bytes.Length);
        }

        private static UdpProtocol CreateUdpProtocol() => new UdpProtocol(
            new DefaultFrameworkProtocol(),
            new ReliableUdpProtocol());
    }
}
