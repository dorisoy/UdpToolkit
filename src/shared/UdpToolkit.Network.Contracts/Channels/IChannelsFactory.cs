namespace UdpToolkit.Network.Contracts.Channels
{
    using System.Collections.Generic;

    /// <summary>
    ///  Channels factory provides a list of available channels for UdpClient.
    /// </summary>
    public interface IChannelsFactory
    {
        /// <summary>
        /// Creates a readonly list of channels.
        /// </summary>
        /// <returns>
        /// Readonly list of channels.
        /// </returns>
        IReadOnlyList<IChannel> CreateChannelsList();
    }
}