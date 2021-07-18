namespace UdpToolkit.Network.Sockets
{
    using System;
    using System.Net;

    internal static class IpExtensions
    {
        internal static int ToInt(this IPAddress ipAddress)
        {
#pragma warning disable SA1503
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
#pragma warning restore

            var ipBytes = ipAddress.GetAddressBytes();
            int intIp = (int)(ipBytes[0] * Math.Pow(2, 24)) + (int)(ipBytes[1] * Math.Pow(2, 16)) + (int)(ipBytes[2] * Math.Pow(2, 8)) + (int)ipBytes[3];
            return IPAddress.HostToNetworkOrder(host: intIp);
        }

        internal static IPEndPoint ToIpEndPoint(this IpV4Address address)
        {
            var ip = new IPAddress(address.Address);

            return new IPEndPoint(ip, address.Port);
        }

        internal static IpV4Address ToIp(this IPEndPoint ipEndPoint)
        {
#pragma warning disable SA1503
            if (ipEndPoint == null) throw new ArgumentNullException(nameof(ipEndPoint));
#pragma warning restore
            return new IpV4Address()
            {
                Address = ipEndPoint.Address.ToInt(),
                Port = (ushort)ipEndPoint.Port,
            };
        }
    }
}