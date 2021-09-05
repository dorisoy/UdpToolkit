namespace UdpToolkit.Network
{
    /// <summary>
    /// Network utils.
    /// </summary>
    public static class NetworkUtils
    {
        /// <summary>
        /// Method for handling wrap around case for packet ids.
        /// </summary>
        /// <param name="s1">Lhs packet id.</param>
        /// <param name="s2">Rhs packet id.</param>
        /// <returns>
        /// true - 's1' > 's2'.
        /// </returns>
        /// <remarks>
        /// https://gafferongames.com/post/reliability_ordering_and_congestion_avoidance_over_udp/.
        /// </remarks>
        public static bool SequenceGreaterThan(ushort s1, ushort s2)
        {
            return ((s1 > s2) && (s1 - s2 <= 32768)) || ((s1 < s2) && (s2 - s1 > 32768));
        }
    }
}