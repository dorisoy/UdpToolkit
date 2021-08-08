namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;

    public sealed class RawUdpChannel : IChannel
    {
        public bool IsReliable { get; } = false;

        public byte ChannelId { get; } = ReliableChannelConsts.RawChannel;

        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            return true;
        }

        public void GetAck(
            ushort id,
            uint acks)
        {
            // no acks for raw udp
        }

        public bool IsDelivered(
            ushort id)
        {
            return true;
        }

        public void HandleOutputPacket(
            out ushort id,
            out uint acks)
        {
            id = default;
            acks = default;
        }

        public bool HandleAck(
            ushort id,
            uint acks)
        {
            return true;
        }
    }
}