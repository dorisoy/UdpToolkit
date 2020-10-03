namespace UdpToolkit.Core
{
    using System;
    using UdpToolkit.Serialization;

    public delegate void ProtocolSubscription(
        byte[] bytes,
        Guid peerId,
        IHost host,
        IPeerManager peerManager,
        ISerializer serializer,
        ITimersPool timersPool,
        IDatagramBuilder datagramBuilder,
        IDateTimeProvider dateTimeProvider);
}