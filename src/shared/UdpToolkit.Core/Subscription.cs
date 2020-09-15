namespace UdpToolkit.Core
{
    using System;
    using UdpToolkit.Serialization;

    public delegate void Subscription(
        byte[] bytes,
        Guid peerId,
        ISerializer serializer,
        IRoomManager roomManager,
        IDatagramBuilder datagramBuilder,
        UdpMode udpMode);
}