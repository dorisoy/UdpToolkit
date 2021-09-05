namespace UdpToolkit.Network.Contracts.Sockets
{
    using System;
    using System.Net;

    /// <summary>
    /// Extensions for deal with IpV4Address.
    /// </summary>
    public static class IpExtensions
    {
        /// <summary>
        /// Convert string ip address to int.
        /// </summary>
        /// <param name="host">String representation of ip address (127.0.0.1).</param>
        /// <returns>
        /// Int representation of ip address.
        /// </returns>
        public static int ToInt(this string host)
        {
            return IPAddress.Parse(host).ToInt();
        }

        /// <summary>
        /// Convert int ip address to string.
        /// </summary>
        /// <param name="address">Int representation of ip address.</param>
        /// <returns>
        /// String representation of ip address.
        /// </returns>
        public static string ToHost(this int address)
        {
            return new IPEndPoint(address, 0).Address.ToString();
        }

        /// <summary>
        /// Convert .NET IpAddress to int.
        /// </summary>
        /// <param name="ipAddress">.NET IpAddress.</param>
        /// <returns>
        /// Int representation of .NET IpAddress.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// ipAddress is null.
        /// </exception>
        internal static int ToInt(this IPAddress ipAddress)
        {
#pragma warning disable SA1503
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
#pragma warning restore

            var ipBytes = ipAddress.GetAddressBytes();
            int intIp = (int)(ipBytes[0] * Math.Pow(2, 24)) + (int)(ipBytes[1] * Math.Pow(2, 16)) + (int)(ipBytes[2] * Math.Pow(2, 8)) + (int)ipBytes[3];
            return IPAddress.HostToNetworkOrder(host: intIp);
        }

        /// <summary>
        /// Convert IpV4Address to .NET IPEndPoint.
        /// </summary>
        /// <param name="address">IpV4Address.</param>
        /// <returns>
        /// .NET IPEndPoint.
        /// </returns>
        internal static IPEndPoint ToIpEndPoint(this IpV4Address address)
        {
            var ip = new IPAddress(address.Address);

            return new IPEndPoint(ip, address.Port);
        }

        /// <summary>
        /// Convert .NET IPEndPoint to IpV4Address.
        /// </summary>
        /// <param name="ipEndPoint">.NET IPEndPoint.</param>
        /// <returns>
        /// IpV4Address.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// ipEndPoint is null.
        /// </exception>
        internal static IpV4Address ToIp(this IPEndPoint ipEndPoint)
        {
#pragma warning disable SA1503
            if (ipEndPoint == null) throw new ArgumentNullException(nameof(ipEndPoint));
#pragma warning restore
            return new IpV4Address(ipEndPoint.Address.ToInt(), (ushort)ipEndPoint.Port);
        }
    }
}