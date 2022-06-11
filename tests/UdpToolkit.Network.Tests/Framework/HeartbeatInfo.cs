namespace UdpToolkit.Network.Tests.Framework
{
    using System;

    internal class HeartbeatInfo
    {
        internal HeartbeatInfo(
            Guid connectionId,
            double rtt)
        {
            ConnectionId = connectionId;
            Rtt = rtt;
        }

        public Guid ConnectionId { get; }

        public double Rtt { get; }
    }
}