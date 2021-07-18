namespace UdpToolkit.Network.Channels
{
    public sealed class RawUdpChannel : IChannel
    {
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