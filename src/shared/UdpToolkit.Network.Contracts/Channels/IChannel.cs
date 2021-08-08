namespace UdpToolkit.Network.Contracts.Channels
{
    public interface IChannel
    {
        bool IsReliable { get; }

        byte ChannelId { get; }

        bool HandleInputPacket(
            ushort id,
            uint acks);

        void HandleOutputPacket(
            out ushort id,
            out uint acks);

        bool HandleAck(
            ushort id,
            uint acks);

        bool IsDelivered(
            ushort id);
    }
}