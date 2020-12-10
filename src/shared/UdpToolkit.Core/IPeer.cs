namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public interface IPeer
    {
        Guid PeerId { get; }

        IPEndPoint GetRandomIp();

        void SetRoomId(
            int roomId);

        void OnPing(
            DateTimeOffset onPingReceived);

        void OnPong(
            DateTimeOffset onPongReceived);

        TimeSpan GetRtt();
    }
}