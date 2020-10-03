namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public interface IPeer
    {
        Guid PeerId { get; }

        List<IPEndPoint> PeerIps { get; }

        IPEndPoint GetRandomIp();

        void OnPing(DateTimeOffset dateTimeOffset);

        void OnPong(DateTimeOffset dateTimeOffset);

        TimeSpan GetRtt();

        bool CanBeHandled();
    }
}