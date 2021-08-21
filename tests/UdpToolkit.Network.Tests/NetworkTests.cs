namespace UdpToolkit.Network.Tests
{
    using FluentAssertions;
    using UdpToolkit.Network.Packets;
    using Xunit;

    public class NetworkTests
    {
        [Fact]
        public void Test()
        {
            var result = PacketType.Connect | PacketType.Ack;
            result.HasFlag(PacketType.Ack)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Test2()
        {
            var result = PacketType.Connect | PacketType.Ack;
            result.ToString()
                .Should()
                .BeEquivalentTo("Connect, Ack");
        }

        [Fact]
        public void Test3()
        {
            var result = PacketType.Connect | PacketType.Ack;
            result
                .Should()
                .NotBe(PacketType.Connect);
        }

        [Fact]
        public void Test4()
        {
            var packetType = PacketType.Connect | PacketType.Ack;

            bool? result = null;
            switch (packetType)
            {
                case PacketType.Connect:
                    result = true;
                    break;
                case PacketType.Connect | PacketType.Ack:
                    result = false;
                    break;
            }

            result.Should().BeFalse();
        }
    }
}