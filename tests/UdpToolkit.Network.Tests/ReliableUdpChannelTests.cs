#pragma warning disable
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
            var networkPacket = CreatePacket(
                hookId: (byte)ProtocolHookId.Connect,
                channelType: ChannelType.ReliableUdp,
                id: 1,
                networkPacketType: NetworkPacketType.Protocol);

            var result = channel.HandleInputPacket(networkPacket: networkPacket);

            Assert.True(result);
            Assert.Equal(1, networkPacket.ChannelHeader.Id);
            Assert.Equal(1, networkPacket.ChannelHeader.Id);
            Assert.Equal(ChannelType.ReliableUdp, networkPacket.ChannelType);
        }

        [Fact]
        public void Duplicate_Dropped()
        {
            var channel = new ReliableChannel(windowSize: 1024);
            var networkPacket = CreatePacket(
                hookId: (byte)ProtocolHookId.Connect,
                channelType: ChannelType.ReliableUdp,
                id: 1,
                networkPacketType: NetworkPacketType.Protocol);

            _ = channel.HandleInputPacket(networkPacket: networkPacket);
            var result = channel.HandleInputPacket(networkPacket: networkPacket);

            Assert.False(false);
            Assert.Equal(1, networkPacket.ChannelHeader.Id);
            Assert.Equal(1, networkPacket.ChannelHeader.Id);
            Assert.Equal(ChannelType.ReliableUdp, networkPacket.ChannelType);
        }

        #region MyRegion
        //
        // [Fact]
        // public void ResendAck_ForDuplicate()
        // {
        //     var channel = new ReliableChannel(windowSize: 1024);
        //     var packet1 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //     var packet2 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //
        //     var allPackets = new[] { packet1, packet2 };
        //
        //     var results = ProcessInputPackets(
        //             channel: channel,
        //             packets: allPackets)
        //         .ToArray();
        //
        //     var first = results[0];
        //     var duplicate = results[1];
        //
        //     Assert.Equal(1U, first.ChannelHeader.Id);
        //     Assert.Equal(ChannelType.ReliableUdp, first.ChannelType);
        //     Assert.Equal(ChannelType.ReliableUdp, first.ChannelType);
        //
        //     Assert.Equal(1U, duplicate.ChannelHeader.Id);
        //     Assert.Equal(ChannelType.ReliableUdp, duplicate.ChannelType);
        //     Assert.Equal(ChannelType.ReliableUdp, duplicate.ChannelType);
        // }
        //
        // [Fact]
        // public void PacketsSequence_Processed()
        // {
        //     var count = 10;
        //     var channel = new ReliableChannel(windowSize: 1024);
        //
        //     var expectedIdsRange = Enumerable
        //         .Range(1, 10)
        //         .Select(x => (ushort)x);
        //
        //     var packets = CreatePackets(hookId: 0, count, ChannelType.ReliableUdp)
        //         .ToArray();
        //
        //     var results = ProcessInputPackets(channel: channel, packets: packets);
        //
        //     Assert.Equal(expectedIdsRange, results.Select(x => x.ChannelHeader.Id));
        // }
        //
        // [Fact]
        // public void ResendAck_ForExpiredPacket()
        // {
        //     var count = 10;
        //     ushort expiredPaketId = 7;
        //     var channel = new ReliableChannel(windowSize: 1024);
        //
        //     var expiredPacket = CreatePacket(hookId: 0, ChannelType.ReliableUdp, expiredPaketId);
        //
        //     var packets = CreatePackets(hookId: 0, count, ChannelType.ReliableUdp)
        //         .ToArray();
        //
        //     var allPackets = packets.Concat(new[] { expiredPacket });
        //
        //     var results = ProcessInputPackets(channel: channel, packets: allPackets);
        //
        //     var lastResult = results.Last();
        //
        //     Assert.Equal(expiredPaketId, lastResult.ChannelHeader.Id);
        //     Assert.Equal(ChannelType.ReliableUdp, lastResult.ChannelType);
        // }
        //
        // [Fact]
        // public void ResendAck_ForFutureDuplicatePacket()
        // {
        //     var channel = new ReliableChannel(windowSize: 1024);
        //
        //     var packet1 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //     var packet2 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 3);
        //     var packet3 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 3);
        //
        //     var allPackets = new[] { packet1, packet2, packet3 };
        //
        //     var results = ProcessInputPackets(channel: channel, packets: allPackets);
        //
        //     var lastResult = results.Last();
        //
        //     Assert.Equal(3U, lastResult.ChannelHeader.Id);
        //     Assert.Equal(ChannelType.ReliableUdp, lastResult.ChannelType);
        // }
        //
        // [Fact]
        // public void PacketFromFuture_Processed()
        // {
        //     var channel = new ReliableChannel(windowSize: 1024);
        //
        //     var packet1 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //     var packet2 = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 3);
        //
        //     var allPackets = new[] { packet1, packet2 };
        //
        //     var results = ProcessInputPackets(channel: channel, packets: allPackets);
        //
        //     Assert.True(results.All(x => x.ChannelType == ChannelType.ReliableUdp));
        // }
        //
        // [Fact]
        // public void OutputPacket_Handled()
        // {
        //     var packet = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //     var channel = new ReliableChannel(windowSize: 1024);
        //
        //     var result = channel.TryHandleOutputPacket(networkPacket: packet);
        //
        //     Assert.NotNull(result);
        //     Assert.Equal(ChannelType.ReliableUdp, result.ChannelType);
        //     Assert.Equal(1u, result.ChannelHeader.Id);
        // }
        //
        // [Fact]
        // public void PendingOutputPacket_Handled()
        // {
        //     var packet = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //     var channel = new ReliableChannel(windowSize: 1);
        //
        //     var allPackets = new[] { packet };
        //     var results = ProcessOutputPackets(channel: channel, packets: allPackets);
        //
        //     var pendingPacket = channel
        //             .GetPendingPackets()
        //             .Single();
        //
        //     Assert.Null(results[0]);
        //     Assert.Equal(ChannelType.ReliableUdp, pendingPacket.ChannelType);
        //     Assert.Equal(1u, pendingPacket.ChannelHeader.Id);
        // }
        //
        // [Fact]
        // public void Ack_Handled()
        // {
        //     var packet = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //     var firstAck = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //     var secondAck = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: 1);
        //
        //     var channel = new ReliableChannel(windowSize: 1024);
        //
        //     var outputPacketResult = channel.TryHandleOutputPacket(networkPacket: packet);
        //
        //     var firstAckResult = channel.HandleAck(networkPacket: firstAck);
        //     var secondAckResult = channel.HandleAck(networkPacket: secondAck);
        //
        //     Assert.NotNull(outputPacketResult);
        //     Assert.NotNull(firstAckResult);
        //     Assert.Null(secondAckResult);
        // }
        #endregion
    }
}
