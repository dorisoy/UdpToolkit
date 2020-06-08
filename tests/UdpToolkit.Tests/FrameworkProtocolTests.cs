namespace UdpToolkit.Tests
{
    using UdpToolkit.Network;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Tests.Utils;
    using Xunit;

    public class FrameworkProtocolTests
    {
        [Fact]
        public void FrameworkHeader_Serialized()
        {
            var header = new FrameworkHeader(
                hubId: Gen.RandomByte(),
                rpcId: Gen.RandomByte());

            var bytes = new DefaultFrameworkProtocol()
                .Serialize(header);

            Assert.Equal(expected: Consts.FrameworkHeaderLength, actual: bytes.Length);
        }

        [Fact]
        public void FrameworkHeader_Deserialized()
        {
            var bytes = new[]
            {
                Gen.RandomByte(),
                Gen.RandomByte(),
                Gen.RandomByte(),
                Gen.RandomByte(),
            };

            var result = new DefaultFrameworkProtocol().TryDeserialize(bytes: bytes, out var header);

            Assert.True(result);
            Assert.NotEqual(expected: default, actual: header);
        }
    }
}
