namespace UdpToolkit.Network
{
    public static class NetworkUtils
    {
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