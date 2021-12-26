namespace UdpToolkit.Network.Tests
{
    using FluentAssertions;
    using UdpToolkit.Network.Contracts;
    using Xunit;

    /*
        The second step 01111111 00000000 00000000 00000001 = 2130706433    (host byte order)
        The third step 00000001 00000000 00000000 01111111 = 16777343       (network byte order)
     */
    public class IpUtilsTests
    {
        [Theory]
        [InlineData("127.0.0.1", 16777343)]
        [InlineData("0.0.0.0", 0)]
        [InlineData("192.168.0.1", 16820416)]
        [InlineData("104.248.135.133", 2240280680)]
        public void ToIntInNetworkByteOrder(string host, uint intInNetworkByteOrder)
        {
            var ip = IpUtils.ToInt(host);

            ip
                .Should()
                .Be(intInNetworkByteOrder);
        }

        [Theory]
        [InlineData("127.0.0.1", 16777343)]
        [InlineData("0.0.0.0", 0)]
        [InlineData("192.168.0.1", 16820416)]
        [InlineData("104.248.135.133", 2240280680)]
        public void ToStringFromIntInNetworkByteOrder(string host, uint intInNetworkByteOrder)
        {
            var ip = IpUtils.ToString(intInNetworkByteOrder);
            ip
                .Should()
                .BeEquivalentTo(host);
        }
    }
}