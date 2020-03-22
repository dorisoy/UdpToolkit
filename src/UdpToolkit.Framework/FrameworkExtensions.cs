namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Annotations;
    using UdpToolkit.Network.Clients;

    public static class FrameworkExtensions
    {
        public static UdpMode Map(UdpChannel mode)
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
