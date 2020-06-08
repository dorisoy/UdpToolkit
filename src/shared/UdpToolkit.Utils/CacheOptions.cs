namespace UdpToolkit.Utils
{
    using System;

    public class CacheOptions
    {
        public CacheOptions(
            TimeSpan roomTtl,
            TimeSpan peerTtl)
        {
            RoomTtl = roomTtl;
            PeerTtl = peerTtl;
        }

        public TimeSpan RoomTtl { get; }

        public TimeSpan PeerTtl { get; }
    }
}