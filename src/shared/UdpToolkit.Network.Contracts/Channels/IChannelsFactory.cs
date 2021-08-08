namespace UdpToolkit.Network.Contracts.Channels
{
    using System.Collections.Generic;

    public interface IChannelsFactory
    {
        IReadOnlyList<IChannel> CreateChannelsList();
    }
}