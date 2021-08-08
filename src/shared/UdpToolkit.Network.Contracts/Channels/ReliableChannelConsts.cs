namespace UdpToolkit.Network.Contracts.Channels
{
    internal static class ReliableChannelConsts
    {
        internal const byte RawChannel = 1;
        internal const byte ReliableChannel = 2;
        internal const byte ReliableOrderedChannel = 3;
        internal const byte SequencedChannel = 4;
    }
}