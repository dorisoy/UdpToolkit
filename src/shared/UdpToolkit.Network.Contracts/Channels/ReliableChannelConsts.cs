namespace UdpToolkit.Network.Contracts.Channels
{
    /// <summary>
    /// Reserved identifiers for channels.
    /// </summary>
    internal static class ReliableChannelConsts
    {
        /// <summary>
        /// RawChannel Id.
        /// </summary>
        internal const byte RawChannel = 252;

        /// <summary>
        /// ReliableChannel Id.
        /// </summary>
        internal const byte ReliableChannel = 253;

        /// <summary>
        /// ReliableOrderedChannel Id.
        /// </summary>
        internal const byte ReliableOrderedChannel = 254;

        /// <summary>
        /// SequencedChannel Id.
        /// </summary>
        internal const byte SequencedChannel = 255;
    }
}