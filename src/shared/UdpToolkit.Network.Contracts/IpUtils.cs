namespace UdpToolkit.Network.Contracts
{
    using System;
    using System.Net;

    /// <summary>
    /// Extensions for deal with Ipv4Address.
    /// </summary>
    public static class IpUtils
    {
        /// <summary>
        /// Convert int ipv4 representation to string.
        /// </summary>
        /// <param name="host">Ipv4 address.</param>
        /// <returns>String representation of ipv4 address.</returns>
        public static string ToString(uint host)
        {
            var bytes = BitConverter.GetBytes(host);
            return new IPAddress(bytes).ToString();
        }

        /// <summary>
        /// Convert string ipv4 representation to int.
        /// </summary>
        /// <param name="host">Ipv4 address.</param>
        /// <returns>Int representation of ipv4 address.</returns>
        public static uint ToInt(string host)
        {
#pragma warning disable SA1503
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore SA1503

            var address = IPAddress.Parse(host);
            byte[] bytes = address.GetAddressBytes();

            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}