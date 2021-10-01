namespace UdpToolkit.Network.Tests.Framework
{
    using System;

    internal class HeartbeatInfo
    {
        internal HeartbeatInfo(
            Guid connectionId,
            TimeSpan rtt)
        {
            ConnectionId = connectionId;
            Rtt = rtt;
        }

        public Guid ConnectionId { get; }

        public TimeSpan Rtt { get; }
    }
}