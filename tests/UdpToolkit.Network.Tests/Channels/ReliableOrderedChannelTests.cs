namespace UdpToolkit.Network.Tests.Channels
{
    using System;
    using FluentAssertions;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Tests.Framework;
    using Xunit;

    public class ReliableOrderedChannelTests
    {
        [Fact]
        public void HasCorrectId()
        {
            var channel = new ReliableOrderedChannel();
            channel.ChannelId
                .Should()
                .Be(254);
        }

        [Fact]
        public void PacketResentOnHeartbeat()
        {
            var channel = new ReliableOrderedChannel();
            channel.IsReliable
                .Should()
                .BeTrue();
        }

        [Fact]
        public void HandleAckThrownNotImplementedException()
        {
            var channel = new ReliableOrderedChannel();
            Action action = () => channel.HandleAck(default);

            action
                .Invoking(a => a())
                .Should()
                .Throw<NotImplementedException>();
        }

        [Fact]
        public void HandleInputPacketThrownNotImplementedException()
        {
            var channel = new ReliableOrderedChannel();
            Action action = () => channel.HandleInputPacket(default);

            action
                .Invoking(a => a())
                .Should()
                .Throw<NotImplementedException>();
        }

        [Fact]
        public void HandleOutputPacketThrownNotImplementedException()
        {
            var channel = new ReliableOrderedChannel();
            Action action = () => channel.HandleOutputPacket(0);

            action
                .Invoking(a => a())
                .Should()
                .Throw<NotImplementedException>();
        }
    }
}