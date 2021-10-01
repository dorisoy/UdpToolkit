namespace UdpToolkit.Network.Tests.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Tests.Framework;
    using Xunit;

    public class ReliableChannelTests
    {
        public static IEnumerable<object[]> ReliableNetworkPackets()
        {
            var connectionId = Guid.NewGuid();
            yield return new object[]
            {
                new List<NetworkHeader>
                {
                    new NetworkHeader(ReliableChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(ReliableChannel.Id, 2, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(ReliableChannel.Id, 3, default, connectionId, PacketType.UserDefined, 1),
                },
                new List<NetworkHeader>()
                {
                    new NetworkHeader(ReliableChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(ReliableChannel.Id, 2, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(ReliableChannel.Id, 3, default, connectionId, PacketType.UserDefined, 1),
                },
                "PacketsInAscendingOrder",
            };

            yield return new object[]
            {
                new List<NetworkHeader>
                {
                    new NetworkHeader(ReliableChannel.Id, 2, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(ReliableChannel.Id, 3, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(ReliableChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                },
                new List<NetworkHeader>()
                {
                    new NetworkHeader(ReliableChannel.Id, 2, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(ReliableChannel.Id, 3, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(ReliableChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                },
                "UnorderedPackets",
            };

            yield return new object[]
            {
                new List<NetworkHeader>()
                {
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                },
                new List<NetworkHeader>()
                {
                    new NetworkHeader(SequencedChannel.Id, 1, default, connectionId, PacketType.UserDefined, 1),
                },
                "PacketsWithDuplicates",
            };
        }

        [Fact]
        public void HasCorrectId()
        {
            var channel = new ReliableChannel(1024);
            channel.ChannelId
                .Should()
                .Be(253);
        }

        [Fact]
        public void PacketResentOnHeartbeat()
        {
            var channel = new ReliableChannel(1024);
            channel.ResendOnHeartbeat
                .Should()
                .BeTrue();
        }

        [Theory]
        [MemberData(nameof(ReliableNetworkPackets))]
        public void OnlyUniquePacketsHandled(
            List<NetworkHeader> incomingPackets,
            List<NetworkHeader> expectedPackets,
            string description)
        {
            var channel = new ReliableChannel(1024);

            var acceptedPackets = incomingPackets
                .Where(header => channel.HandleInputPacket(header))
                .ToList();

            acceptedPackets
                .Should()
                .BeEquivalentTo(expectedPackets, options => options.WithStrictOrdering(), description);
        }

        [Fact]
        public void DuplicateAckPacketsDropped()
        {
            var channel = new ReliableChannel(1024);

            var producedPackets = Enumerable
                .Range(10, 100)
                .Select(_ => channel.HandleOutputPacket(
                    dataType: Gen.RandomByte(),
                    connectionId: Gen.RandomGuid(),
                    packetType: Gen.RandomEnum<PacketType>()))
                .ToList();

            var handledAcksFirstTime = producedPackets
                .Where(p => channel.HandleAck(p))
                .ToList();

            var handledAcksSecondTime = producedPackets
                .Where(p => channel.HandleAck(p))
                .ToList();

            handledAcksFirstTime
                .Should()
                .BeEquivalentTo(producedPackets, options => options.WithStrictOrdering());

            handledAcksSecondTime
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void PacketsWithoutAckNotDelivered()
        {
            var channel = new ReliableChannel(1024);

            var producedPackets = Enumerable
                .Range(10, 100)
                .Select(_ => channel.HandleOutputPacket(
                    dataType: Gen.RandomByte(),
                    connectionId: Gen.RandomGuid(),
                    packetType: Gen.RandomEnum<PacketType>()))
                .ToList();

            var handledAcksFirstTime = producedPackets
                .Where(p => channel.IsDelivered(p))
                .ToList();

            handledAcksFirstTime
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void PacketsWithAckDelivered()
        {
            var channel = new ReliableChannel(1024);

            var producedPackets = Enumerable
                .Range(10, 100)
                .Select(_ => channel.HandleOutputPacket(
                    dataType: Gen.RandomByte(),
                    connectionId: Gen.RandomGuid(),
                    packetType: Gen.RandomEnum<PacketType>()))
                .ToList();

            var handledPackets = producedPackets
                .Where(p => channel.HandleAck(p))
                .ToList();

            var deliveredPackets = handledPackets
                .Where(p => channel.IsDelivered(p))
                .ToList();

            deliveredPackets
                .Should()
                .BeEquivalentTo(producedPackets, options => options.WithStrictOrdering());
        }

        [Fact]
        public void PacketsHandledAfterNetWindowWrapAround()
        {
            var networkWindowSize = Gen.RandomInt(100, 1000);
            var channel = new ReliableChannel(networkWindowSize);

            var firstBucket = Enumerable
                .Range(0, networkWindowSize)
                .Select(_ => channel.HandleOutputPacket(
                    dataType: Gen.RandomByte(),
                    connectionId: Gen.RandomGuid(),
                    packetType: Gen.RandomEnum<PacketType>()))
                .ToList();

            var secondBucket = Enumerable
                .Range(0, networkWindowSize)
                .Select(_ => channel.HandleOutputPacket(
                    dataType: Gen.RandomByte(),
                    connectionId: Gen.RandomGuid(),
                    packetType: Gen.RandomEnum<PacketType>()))
                .ToList();

            firstBucket
                .Where(p => channel.HandleAck(p))
                .ToList()
                .Count
                .Should()
                .Be(networkWindowSize);

            secondBucket
                .Where(p => channel.HandleAck(p))
                .ToList()
                .Count
                .Should()
                .Be(networkWindowSize);
        }
    }
}