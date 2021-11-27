namespace UdpToolkit.Network.Tests.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Tests.Framework;
    using Xunit;

    public class SequencedChannelTests
    {
        public static IEnumerable<object[]> SequencedNetworkPackets()
        {
            var connectionId = Guid.NewGuid();
            yield return new object[]
            {
                new List<NetworkHeader>
                {
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 2, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 3, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 2),
                    new NetworkHeader(SequencedChannel.Id, 2, default, connectionId, PacketType.UserDefined, 2),
                    new NetworkHeader(SequencedChannel.Id, 3, default, connectionId, PacketType.UserDefined, 2),
                },
                new List<NetworkHeader>()
                {
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 2, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 3, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 2),
                    new NetworkHeader(SequencedChannel.Id, 2, default, connectionId, PacketType.UserDefined, 2),
                    new NetworkHeader(SequencedChannel.Id, 3, default, connectionId, PacketType.UserDefined, 2),
                },
                "PacketsInAscendingOrder",
            };

            yield return new object[]
            {
                new List<NetworkHeader>()
                {
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 2),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 2),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 2),
                },
                new List<NetworkHeader>()
                {
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 2),
                },
                "PacketsWithDuplicates",
            };

            yield return new object[]
            {
                new List<NetworkHeader>
                {
                    new NetworkHeader(SequencedChannel.Id, 3, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 2, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 3, default, connectionId, PacketType.UserDefined, 2),
                    new NetworkHeader(SequencedChannel.Id, 2, default, connectionId, PacketType.UserDefined, 2),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 2),
                },
                new List<NetworkHeader>()
                {
                    new NetworkHeader(SequencedChannel.Id, 3, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 3, default, connectionId, PacketType.UserDefined, 2),
                },
                "PacketsInDescendingOrder",
            };
        }

        [Fact]
        public void HasCorrectId()
        {
            var channel = new SequencedChannel(sequences: new ushort[ushort.MaxValue]);
            channel.ChannelId
                .Should()
                .Be(255);
        }

        [Fact]
        public void NotResendOnHeartbeat()
        {
            var channel = new SequencedChannel(sequences: new ushort[ushort.MaxValue]);
            channel.IsReliable
                .Should()
                .BeFalse();
        }

        [Theory]
        [MemberData(nameof(SequencedNetworkPackets))]
        public void HandledOnlyMostNewerPackets(
            List<NetworkHeader> incomingPackets,
            List<NetworkHeader> expectedPackets,
            string description)
        {
            var channel = new SequencedChannel(sequences: new ushort[ushort.MaxValue]);

            var handledPackets = incomingPackets
                .Where(packet => channel.HandleInputPacket(packet))
                .ToList();

            handledPackets
                .Should()
                .BeEquivalentTo(expectedPackets, options => options.WithStrictOrdering(), description);
        }

        [Fact]
        public void AnyAckPacketHandled()
        {
            var channel = new SequencedChannel(sequences: new ushort[ushort.MaxValue]);
            var randomPackets = Gen.GenerateRandomPackets();

            var handledPackets = randomPackets
                .Where(packet => channel.HandleAck(packet))
                .ToList();

            handledPackets
                .Should()
                .BeEquivalentTo(randomPackets, options => options.WithStrictOrdering());
        }

        [Fact]
        public void IdsIncreasesMonotonically()
        {
            var channel = new SequencedChannel(sequences: new ushort[ushort.MaxValue]);
            var packetsCount = Gen.RandomInt(10, 100);

            var networkPacketIds = Enumerable
                .Range(0, packetsCount)
                .Select(_ => channel.HandleOutputPacket(0))
                .ToList();

            var expectedIds = Enumerable
                .Range(1, packetsCount)
                .ToArray();

            networkPacketIds
                .Should()
                .BeEquivalentTo(expectedIds, options => options.WithStrictOrdering());
        }
    }
}