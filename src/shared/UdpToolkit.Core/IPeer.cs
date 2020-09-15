namespace UdpToolkit.Core
{
    using System;
    using System.Net;

    public interface IPeer
    {
        Guid PeerId { get; }

        IPEndPoint GetRandomIp();
    }
}