namespace UdpToolkit.Network.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;

    public static class Utils
    {
        public static NetworkPacket CreatePacket(byte hookId, ChannelType channelType, ushort id) => new NetworkPacket(
            peerId: Guid.NewGuid(),
            channelHeader: new ChannelHeader(
                id: id,
                acks: 0),
            serializer: () => new byte[] { 1, 2, 3 },
            ipEndPoint: new IPEndPoint(
                address: IPAddress.Loopback,
                port: 123),
            channelType: channelType,
            hookId: hookId);

        public static IEnumerable<NetworkPacket> CreatePackets(byte hookId, int count, ChannelType channelType) => Enumerable
            .Range(1, count)
            .Select(_ => CreatePacket(hookId, channelType, (ushort)_));

        public static List<ChannelResult> ProcessInputPackets(
            IChannel channel,
            IEnumerable<NetworkPacket> packets)
        {
            return packets
                .Select(channel.TryHandleInputPacket)
                .ToList();
        }

        public static List<NetworkPacket> ProcessOutputPackets(
            IChannel channel,
            IEnumerable<NetworkPacket> packets)
        {
            return packets
                .Select(channel.TryHandleOutputPacket)
                .ToList();
        }
    }
}