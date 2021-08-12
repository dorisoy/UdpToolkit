namespace UdpToolkit.Network.Queues
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;

    internal interface IResendQueue
    {
        void Add(
            Guid connectionId,
            PendingPacket pendingPacket);

        public List<PendingPacket> Get(
            Guid connectionId);
    }
}