namespace UdpToolkit.Framework.Client.Events
{
    using System;
    using UdpToolkit.Core;

    public readonly struct ProducedEvent
    {
        public ProducedEvent(
            byte roomId,
            EventDescriptor eventDescriptor,
            Func<ISerializer, byte[]> serialize)
        {
            RoomId = roomId;
            EventDescriptor = eventDescriptor;
            Serialize = serialize;
        }

        public byte RoomId { get; }

        public EventDescriptor EventDescriptor { get; }

        public Func<ISerializer, byte[]> Serialize { get; }
    }
}