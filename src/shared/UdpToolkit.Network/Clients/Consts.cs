namespace UdpToolkit.Network.Clients
{
    using System;
    using UdpToolkit.Network.Contracts.Protocol;

    /// <summary>
    /// Network consts.
    /// </summary>
    internal static unsafe class Consts
    {
        /// <summary>
        /// Size of network header.
        /// </summary>
        public static readonly int NetworkHeaderSize = sizeof(NetworkHeader);

        /// <summary>
        /// Size of routing key.
        /// </summary>
        public static readonly int RoutingKeySize = sizeof(Guid);
    }
}