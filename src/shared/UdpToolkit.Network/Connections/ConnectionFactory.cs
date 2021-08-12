namespace UdpToolkit.Network.Connections
{
    using System;
    using System.Linq;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    internal sealed class ConnectionFactory : IConnectionFactory
    {
        private readonly IChannelsFactory _channelsFactory;

        internal ConnectionFactory(
            IChannelsFactory channelsFactory)
        {
            _channelsFactory = channelsFactory;
        }

        public IConnection Create(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            IpV4Address ipAddress)
        {
            var outputChannels = _channelsFactory
                .CreateChannelsList()
                .ToDictionary(channel => channel.ChannelId, channel => channel);

            var inputChannels = _channelsFactory
                .CreateChannelsList()
                .ToDictionary(channel => channel.ChannelId, channel => channel);

            return new Connection(
                connectionId: connectionId,
                keepAlive: keepAlive,
                lastHeartbeat: lastHeartbeat,
                ipAddress: ipAddress,
                outputChannels: outputChannels,
                inputChannels: inputChannels);
        }
    }
}