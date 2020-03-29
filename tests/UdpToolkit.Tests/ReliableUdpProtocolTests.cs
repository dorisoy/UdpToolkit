namespace UdpToolkit.Tests
{
    using System.Linq;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Tests.Utils;
    using Xunit;

    public class ReliableUdpProtocolTests
    {
        [Fact]
        public void ReliableUdpHeader_Serialized()
        {
            var header = new ReliableUdpHeader(
                localNumber: Gen.RandomUint(),
                ack: Gen.RandomUint(),
                acks: Gen.RandomUint());

            var protocol = new ReliableUdpProtocol().Serialize(header);

            Assert.Equal(expected: Consts.ReliableUdpProtocolHeaderLength, actual: protocol.Length);
        }

        [Fact]
        public void ReliableUdpHeader_Deserialized()
        {
            var bytes = Enumerable
                .Range(0, Consts.ReliableUdpProtocolHeaderLength)
                .Select(_ => Gen.RandomByte())
                .ToArray();

            var result = new ReliableUdpProtocol().TryDeserialize(bytes, out var header);

            Assert.True(result);
            Assert.NotEqual(expected: default, actual: header);
        }
    }
}
