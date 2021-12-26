namespace UdpToolkit.Network.Contracts
{
    using System;
    using System.Runtime.InteropServices;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Extensions for deal with Ipv4Address.
    /// </summary>
    public static class IpUtils
    {
        private const string LibName = "udp_toolkit_native";

        /// <summary>
        /// Convert int ipv4 representation to string.
        /// </summary>
        /// <param name="host">Ipv4 address.</param>
        /// <returns>String representation of ipv4 address.</returns>
        public static string ToString(uint host)
        {
            var ptr = ToStringNative(host);
            return Marshal.PtrToStringAnsi(ptr);
        }

        /// <summary>
        /// Convert IpV4Address to string.
        /// </summary>
        /// <param name="ipV4Address">Instance of IpV4Address.</param>
        /// <returns>String representation of ipv4 address.</returns>
        public static string ToString(IpV4Address ipV4Address)
        {
            return $"{ToString(ipV4Address.Address)}:{ipV4Address.Port}";
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

            return ToIntNative(host);
        }

        [DllImport(LibName, EntryPoint = "to_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint ToIntNative(string host);

        [DllImport(LibName, EntryPoint = "to_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ToStringNative(uint host);
    }
}