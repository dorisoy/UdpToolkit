namespace UdpToolkit.Core
{
    using System;
    using System.Net;

    public interface IPeer
    {
        Guid PeerId { get; }

        IPEndPoint GetRandomIp();

        void OnPing(DateTimeOffset dateTimeOffset);

        void OnPong(DateTimeOffset dateTimeOffset);

        TimeSpan GetRtt();
    }
}