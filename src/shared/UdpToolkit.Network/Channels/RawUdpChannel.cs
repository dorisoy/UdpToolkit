namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Packets;

    public sealed class RawUdpChannel : IChannel
    {
        public bool HandleInputPacket(
            NetworkPacket networkPacket)
        {
            return true;
        }

        public void GetAck(
            NetworkPacket networkPacket)
        {
        }

        public bool IsDelivered(
            ushort networkPacketId)
        {
            return true;
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            return true;
        }
    }
}