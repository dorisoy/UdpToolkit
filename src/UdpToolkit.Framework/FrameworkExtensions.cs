namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Annotations;
    using UdpToolkit.Network.Clients;

    public static class FrameworkExtensions
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

        public static UdpToolkit.Network.Clients.UdpMode Map(this UdpToolkit.Core.UdpMode mode)
        {
            switch (mode)
            {
                case UdpToolkit.Core.UdpMode.Udp:
                    return UdpToolkit.Network.Clients.UdpMode.Udp;

                case UdpToolkit.Core.UdpMode.ReliableUdp:
                    return UdpToolkit.Network.Clients.UdpMode.ReliableUdp;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static UdpToolkit.Core.UdpMode Map(this UdpToolkit.Network.Clients.UdpMode mode)
        {
            switch (mode)
            {
                case UdpToolkit.Network.Clients.UdpMode.Udp:
                    return UdpToolkit.Core.UdpMode.Udp;

                case UdpToolkit.Network.Clients.UdpMode.ReliableUdp:
                    return UdpToolkit.Core.UdpMode.ReliableUdp;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
