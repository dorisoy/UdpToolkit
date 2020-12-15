namespace UdpToolkit.Network.Tests
{
    using System;
    using UdpToolkit.Network.Channels;
    using Xunit;
    using static Utils;

    public class NetWindowTests
    {
        [Fact]
        public void IsEmpty()
        {
            var netWindow = new NetWindow(windowSize: 0);
            var result = netWindow.CanSet(id: 0);

            Assert.False(result);
        }

        [Fact]
        public void CanSet()
        {
            var windowSize = 1024;
            var netWindow = new NetWindow(windowSize: windowSize);

            for (ushort i = 0; i < windowSize; i++)
            {
                var result = netWindow.CanSet(i);
                Assert.True(result);
            }
        }

        [Fact]
        public void IsFull()
        {
            var windowSize = 1024;
            var netWindow = new NetWindow(windowSize: windowSize);

            for (ushort i = 0; i < windowSize; i++)
            {
                var packet = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: i, networkPacketType: NetworkPacketType.FromClient);

                // netWindow.InsertPacketData(packet, true);
            }

            for (ushort i = 0; i < windowSize; i++)
            {
                var result = netWindow.CanSet(i);
                Assert.False(result);
            }
        }

        [Fact(Skip = "todo")]
        public void Killed()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Rotated()
        {
            var windowSize = 1024;
            var netWindow = new NetWindow(windowSize: windowSize);

            for (ushort i = 0; i < windowSize * 2; i++)
            {
                // var packet = CreatePacket(hookId: 0, channelType: ChannelType.ReliableUdp, id: i, networkPacketType: NetworkPacketType.FromClient);
                // netWindow.InsertPacketData(packet, true);
            }

            for (ushort i = 1024; i < windowSize * 2; i++)
            {
                // netWindow.TryGetNetworkPacket(i, out var packet);
                // Assert.Equal(i, packet.Value.Id);
            }
        }
    }
}