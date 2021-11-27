namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Channels;

    /// <inheritdoc />
    public sealed class ChannelsFactory : IChannelsFactory
    {
        /// <inheritdoc />
        public IReadOnlyList<IChannel> CreateChannelsList()
        {
            var sequencesBuffer = new ushort[ushort.MaxValue];

            return new List<IChannel>
            {
                new RawUdpChannel(),
                new ReliableChannel(1024),
                new SequencedChannel(sequences: sequencesBuffer),
            };
        }
    }
}