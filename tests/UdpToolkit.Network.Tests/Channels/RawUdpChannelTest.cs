namespace UdpToolkit.Network.Tests.Channels
{
    using FluentAssertions;
    using UdpToolkit.Network.Channels;
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
            channel.ResendOnHeartbeat
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
        public void AnyPacketIsDelivered()
        {
            var channel = new RawUdpChannel();
            var packet = Gen.GenerateRandomPacket();

            channel
                .IsDelivered(packet)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void OutputPacketWellFormed()
        {
            var channel = new RawUdpChannel();

            var dataType = Gen.RandomByte();
            var connectionId = Gen.RandomGuid();
            var packetType = Gen.RandomEnum<PacketType>();

            var packet = channel.HandleOutputPacket(
                dataType: dataType,
                connectionId: connectionId,
                packetType: packetType);

            var expected = new NetworkHeader(
                channelId: RawUdpChannel.Id,
                id: 0,
                acks: 0,
                connectionId: connectionId,
                packetType: packetType,
                dataType: dataType);

            packet
                .Should()
                .BeEquivalentTo(expected);
        }
    }
}