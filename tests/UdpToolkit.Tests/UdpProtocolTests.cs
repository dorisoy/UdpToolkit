namespace UdpToolkit.Tests
{
    using System.Linq;
    using System.Net;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Tests.Utils;
    using UdpToolkit.Utils;
    using Xunit;

    public class UdpProtocolTests
    {
        [Fact]
        public void UdpProtocol_TryGetInputPacket_InputPacket_Deserialized()
        {
            var protocol = CreateUdpProtocol(new DateTimeProvider());
            var bytes = new byte[5];

            // TODO more cases
            // framework header
            bytes[0] = 0;
            bytes[0] = 0;
            bytes[0] = 0;
            bytes[0] = 0;

            // packet type
            bytes[Consts.PacketTypeIndex] = (byte)PacketType.Udp;

            var result = protocol.TryGetInputPacket(bytes: bytes, ipEndPoint: IPEndPoint.Parse("0.0.0.0"), networkPacket: out var packet);

            Assert.True(result);
        }

        [Fact]
        public void UdpProtocol_GetBytes_NetworkPacket_Serialized()
        {
            var protocol = CreateUdpProtocol(new DateTimeProvider());

            var payload = Enumerable
                .Range(0, Gen.RandomUshort(10, 100))
                .Select(_ => Gen.RandomByte())
                .ToArray();

            var fh = new FrameworkHeader(
                hubId: Gen.RandomByte(),
                rpcId: Gen.RandomByte(),
                roomId: Gen.RandomUshort());

            var rh = new ReliableUdpHeader(
                localNumber: Gen.RandomUint(),
                ack: Gen.RandomUint(),
                acks: Gen.RandomUint());

            var networkPacket = new NetworkPacket(
                payload: payload,
                ipEndPoint: IPEndPoint.Parse("0.0.0.0"),
                udpMode: UdpMode.Udp,
                frameworkHeader: fh);

            var bytes = protocol.GetBytes(networkPacket, rh);

            Assert.NotEmpty(bytes);
        }

        private static UdpProtocol CreateUdpProtocol(IDateTimeProvider dateTimeProvider) => new UdpProtocol(
            new DefaultFrameworkProtocol(),
            new ReliableUdpProtocol(),
            dateTimeProvider);
    }
}
