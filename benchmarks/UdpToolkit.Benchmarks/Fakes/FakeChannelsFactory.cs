namespace UdpToolkit.Benchmarks.Fakes
{
    using System.Collections.Generic;
    using UdpToolkit.Benchmarks.Fakes;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Utils;

    public class FakeChannelsFactory : IChannelsFactory
    {
        private readonly IReadOnlyList<IChannel> _channels;

        public FakeChannelsFactory()
        {
            var netWindowSize = 1024;
            _channels = new List<IChannel>()
            {
                new ReliableChannel(netWindowSize),
            };
        }

        public IReadOnlyList<IChannel> CreateChannelsList()
        {
            return _channels;
        }
    }
}