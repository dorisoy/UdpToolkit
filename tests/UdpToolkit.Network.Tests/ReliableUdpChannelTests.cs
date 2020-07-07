namespace UdpToolkit.Network.Tests
{
    using System.Linq;
    using UdpToolkit.Network.Channels;
    using Xunit;
    using static Utils;

    public class ReliableUdpChannelTests
    {
        [Fact]
        public void Packet_Processed()
        {
            var channel = new ReliableChannel(windowSize: 1024);
            var networkPacket = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);

            var result = channel.TryHandleInputPacket(networkPacket: networkPacket);

            Assert.Equal(ChannelState.Accepted, result.ChannelState);
            Assert.Equal(ChannelType.ReliableUdp, result.NetworkPacket.ChannelType);
            Assert.Equal(1, result.NetworkPacket.ChannelHeader.Id);
        }

        [Fact]
        public void ResendAck_ForDuplicate()
        {
            var channel = new ReliableChannel(windowSize: 1024);
            var packet1 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
            var packet2 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);

            var allPackets = new[] { packet1, packet2 };

            var results = ProcessInputPackets(
                    channel: channel,
                    packets: allPackets)
                .ToArray();

            var first = results[0];
            var duplicate = results[1];

            Assert.Equal(ChannelState.Accepted, first.ChannelState);
            Assert.Equal(ChannelState.Resend, duplicate.ChannelState);

            Assert.Equal(1U, first.NetworkPacket.ChannelHeader.Id);
            Assert.Equal(ChannelType.ReliableUdp, first.NetworkPacket.ChannelType);
            Assert.Equal(ChannelType.ReliableUdp, first.NetworkPacket.ChannelType);

            Assert.Equal(1U, duplicate.NetworkPacket.ChannelHeader.Id);
            Assert.Equal(ChannelType.ReliableUdp, duplicate.NetworkPacket.ChannelType);
            Assert.Equal(ChannelType.ReliableUdp, duplicate.NetworkPacket.ChannelType);
        }

        [Fact]
        public void PacketsSequence_Processed()
        {
            var count = 10;
            var channel = new ReliableChannel(windowSize: 1024);

            var expectedIdsRange = Enumerable
                .Range(1, 10)
                .Select(x => (ushort)x);

            var packets = CreatePackets(hookId: 0, count, ChannelType.ReliableUdp)
                .ToArray();

            var results = ProcessInputPackets(channel: channel, packets: packets);

            Assert.True(results.All(x => x.ChannelState == ChannelState.Accepted));
            Assert.Equal(expectedIdsRange, results.Select(x => x.NetworkPacket.ChannelHeader.Id));
        }

        [Fact]
        public void ResendAck_ForExpiredPacket()
        {
            var count = 10;
            ushort expiredPaketId = 7;
            var channel = new ReliableChannel(windowSize: 1024);

            var expiredPacket = CreatePacket(hookId: 0, ChannelType.ReliableUdp, expiredPaketId);

            var packets = CreatePackets(hookId: 0, count, ChannelType.ReliableUdp)
                .ToArray();

            var allPackets = packets.Concat(new[] { expiredPacket });

            var results = ProcessInputPackets(channel: channel, packets: allPackets);

            var lastResult = results.Last();

            Assert.Equal(expiredPaketId, lastResult.NetworkPacket.ChannelHeader.Id);
            Assert.Equal(ChannelType.ReliableUdp, lastResult.NetworkPacket.ChannelType);
            Assert.True(results.Take(count).All(x => x.ChannelState == ChannelState.Accepted));
        }

        [Fact]
        public void ResendAck_ForFutureDuplicatePacket()
        {
            var channel = new ReliableChannel(windowSize: 1024);

            var packet1 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
            var packet2 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 3);
            var packet3 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 3);

            var allPackets = new[] { packet1, packet2, packet3 };

            var results = ProcessInputPackets(channel: channel, packets: allPackets);

            var lastResult = results.Last();

            Assert.Equal(3U, lastResult.NetworkPacket.ChannelHeader.Id);
            Assert.Equal(ChannelType.ReliableUdp, lastResult.NetworkPacket.ChannelType);
            Assert.Equal(ChannelState.Resend, lastResult.ChannelState);
        }

        [Fact]
        public void PacketFromFuture_Processed()
        {
            var channel = new ReliableChannel(windowSize: 1024);

            var packet1 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
            var packet2 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 3);

            var allPackets = new[] { packet1, packet2 };

            var results = ProcessInputPackets(channel: channel, packets: allPackets);

            Assert.True(results.All(x => x.NetworkPacket.ChannelType == ChannelType.ReliableUdp));
            Assert.True(results.All(x => x.ChannelState == ChannelState.Accepted));
        }

        [Fact]
        public void OutputPacket_Handled()
        {
            var packet = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
            var channel = new ReliableChannel(windowSize: 1024);

            var result = channel.TryHandleOutputPacket(networkPacket: packet);

            Assert.True(result.HasValue);
            Assert.Equal(ChannelType.ReliableUdp, result.Value.ChannelType);
            Assert.Equal(1u, result.Value.ChannelHeader.Id);
        }

        [Fact]
        public void PendingOutputPacket_Handled()
        {
            var packet = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
            var channel = new ReliableChannel(windowSize: 1);

            var allPackets = new[] { packet };
            var results = ProcessOutputPackets(channel: channel, packets: allPackets);

            var pendingPacket = channel
                    .GetPendingPackets()
                    .Single();

            Assert.False(results[0].HasValue);
            Assert.Equal(ChannelType.ReliableUdp, pendingPacket.ChannelType);
            Assert.Equal(1u, pendingPacket.ChannelHeader.Id);
        }

        [Fact]
        public void Ack_Handled()
        {
            var packet = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
            var firstAck = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
            var secondAck = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);

            var channel = new ReliableChannel(windowSize: 1024);

            var outputPacketResult = channel.TryHandleOutputPacket(networkPacket: packet);

            var firstAckResult = channel.HandleAck(networkPacket: firstAck);
            var secondAckResult = channel.HandleAck(networkPacket: secondAck);

            Assert.True(outputPacketResult.HasValue);
            Assert.True(firstAckResult.HasValue);
            Assert.False(secondAckResult.HasValue);
        }
    }
}
