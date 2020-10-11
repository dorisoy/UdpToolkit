namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public interface IPeer
    {
        Guid PeerId { get; }

        IPEndPoint GetRandomIp();

        ushort GetRoomId();

        void SetRoomId(
            ushort roomId);

        void OnPing(
            DateTimeOffset onPingReceived);

        void OnPong(
            DateTimeOffset onPongReceived);

        void OnActivity(
            DateTimeOffset lastActivityAt);

        TimeSpan GetRtt();
    }
}