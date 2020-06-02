namespace UdpToolkit.Framework.Client
{
    using System;
    using UdpToolkit.Annotations;
    using UdpToolkit.Network.Clients;

    public static class ClientUtils
    {
        public static UdpMode Map(this UdpChannel mode)
        {
            switch (mode)
            {
                case UdpChannel.Udp:
                    return UdpMode.Udp;

                case UdpChannel.ReliableUdp:
                    return UdpMode.ReliableUdp;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
