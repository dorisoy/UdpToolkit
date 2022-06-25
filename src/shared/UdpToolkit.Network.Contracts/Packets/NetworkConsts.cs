namespace UdpToolkit.Network.Contracts.Packets
{
    /// <summary>
    /// Network constants.
    /// </summary>
    public static class NetworkConsts
    {
        /// <summary>
        /// Connect identifier.
        /// </summary>
        public const byte Connect = byte.MaxValue;

        /// <summary>
        /// Disconnect identifier.
        /// </summary>
        public const byte Disconnect = byte.MaxValue - 1;

        /// <summary>
        /// Ping identifier.
        /// </summary>
        public const byte Ping = byte.MaxValue - 2;
    }
}