namespace UdpToolkit.Network
{
    using System.Net;
    using System.Net.Sockets;

    public static class NetworkUtils
    {
        private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);

        public static int GetAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(DefaultLoopbackEndpoint);
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        // https://gafferongames.com/post/reliability_ordering_and_congestion_avoidance_over_udp/
        public static bool SequenceGreaterThan(ushort s1, ushort s2)
        {
            return ((s1 > s2) && (s1 - s2 <= 32768)) || ((s1 < s2) && (s2 - s1 > 32768));
        }

        public static void SetBitValue(ref uint container, int bitPosition)
        {
            container |= 1u << bitPosition;
        }
    }
}