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

        public static string BitsAsString(uint container)
        {
            char[] b = new char[32];
            uint pos = 31;
            int i = 0;

            while (i < 32)
            {
                if ((container & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }

                pos--;
                i++;
            }

            return new string(b);
        }

        public static bool ContainBit(uint container, int bitPosition)
        {
            return (container & (1 << bitPosition)) != 0;
        }

        public static void SetBitValue(ref uint container, int bitPosition)
        {
            container |= 1u << bitPosition;
        }
    }
}