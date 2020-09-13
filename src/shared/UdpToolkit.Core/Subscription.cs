namespace UdpToolkit.Core
{
    using System;
    using UdpToolkit.Serialization;

    public delegate void Subscription(
        byte[] bytes,
        Guid peerId,
        ISerializer serializer,
        IDataGramBuilder dataGramBuilder,
        UdpMode udpMode);
}