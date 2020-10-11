namespace UdpToolkit.Framework
{
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;

    public interface IBroadcastStrategy
    {
        BroadcastType Type { get; }

        void Execute(
            ushort roomId,
            NetworkPacket networkPacket);
    }
}