namespace UdpToolkit.Network.Channels
{
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Channels;

    public sealed class ChannelsFactory : IChannelsFactory
    {
        public IReadOnlyList<IChannel> CreateChannelsList()
        {
            return new List<IChannel>
            {
                new RawUdpChannel(),
                new ReliableChannel(windowSize: 1024),
                new ReliableOrderedChannel(),
                new SequencedChannel(),
            };
        }
    }
}