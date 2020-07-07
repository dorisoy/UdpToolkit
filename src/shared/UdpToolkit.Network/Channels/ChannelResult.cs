namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Packets;

    public readonly struct ChannelResult
    {
        public ChannelResult(
            ChannelState channelState,
            NetworkPacket networkPacket)
        {
            ChannelState = channelState;
            NetworkPacket = networkPacket;
        }

        public ChannelState ChannelState { get; }

        public NetworkPacket NetworkPacket { get; }
    }
}