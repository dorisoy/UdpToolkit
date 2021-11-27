namespace UdpToolkit.Network.Tests.Channels
{
    using System;
    using FluentAssertions;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Tests.Framework;
    using Xunit;

    public class RawUdpChannelTest
    {
        [Fact]
        public void HasCorrectId()
        {
            var channel = new RawUdpChannel();
            channel.ChannelId
                .Should()
                .Be(252);
        }

        [Fact]
        public void PacketNotResentOnHeartbeat()
        {
            var channel = new RawUdpChannel();
            channel.IsReliable
                .Should()
                .BeFalse();
        }

        [Fact]
        public void AnyInputPacketHandled()
        {
            var channel = new RawUdpChannel();
            var packet = Gen.GenerateRandomPacket();

            channel
                .HandleInputPacket(packet)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void AnyAckPacketHandled()
        {
            var channel = new RawUdpChannel();
            var packet = Gen.GenerateRandomPacket();

            channel
                .HandleAck(packet)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void OutputPacketWellFormed()
        {
            var channel = new RawUdpChannel();

            var networkPacketId = channel.HandleOutputPacket(0);

            networkPacketId
                .Should()
                .Be(0);
        }
    }
}