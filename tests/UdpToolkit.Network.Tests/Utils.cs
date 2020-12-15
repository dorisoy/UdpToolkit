namespace UdpToolkit.Network.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;

    public static class Utils
    {
        public static PooledObject<NetworkPacket> CreatePacket(byte hookId, ChannelType channelType, NetworkPacketType networkPacketType, ushort id) => throw new NotImplementedException();

        public static IEnumerable<NetworkPacket> CreatePackets(byte hookId, int count, ChannelType channelType, NetworkPacketType networkPacketType) => throw new NotImplementedException();

        public static List<bool> ProcessInputPackets(
            IChannel channel,
            IEnumerable<NetworkPacket> packets)
        {
            throw new NotImplementedException();

            // return packets
            //     .Select(channel.HandleInputPacket)
            //     .ToList();
        }

        public static void ProcessOutputPackets(
            IChannel channel,
            IEnumerable<NetworkPacket> packets)
        {
        }
    }
}