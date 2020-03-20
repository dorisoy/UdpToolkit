using UdpToolkit.Network;
using UdpToolkit.Network.Protocol;
using UdpToolkit.Tests.Utils;
using Xunit;

namespace UdpToolkit.Tests
{
    public class FrameworkProtocolTests
    {
        [Fact]
        public void FrameworkHeader_Serialized()
        {
            var header = new FrameworkHeader(
                hubId: Gen.GetRandomByte(),
                rpcId: Gen.GetRandomByte(),
                scopeId: Gen.GetRandomUshort());

            var bytes = new DefaultFrameworkProtocol()
                .Serialize(header);
            
            Assert.Equal(expected: Consts.FrameworkHeaderLength, actual: bytes.Length);
        }
        
        [Fact]
        public void FrameworkHeader_Deserialized()
        {
            var bytes = new []
            {
                Gen.GetRandomByte(),
                Gen.GetRandomByte(),
                Gen.GetRandomByte(),
                Gen.GetRandomByte(),
            };
            
            var result = new DefaultFrameworkProtocol().TryDeserialize(bytes: bytes, out var header);
            
            Assert.True(result);
            Assert.NotEqual(expected: default, actual: header);
        }
    }
}
