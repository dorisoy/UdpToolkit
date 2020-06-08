namespace UdpToolkit.Framework.Client.Events
{
    using System;
    using UdpToolkit.Core;

    public readonly struct ProducedEvent
    {
        public ProducedEvent(
            EventDescriptor eventDescriptor,
            Func<ISerializer, byte[]> serialize)
        {
            EventDescriptor = eventDescriptor;
            Serialize = serialize;
        }

        public EventDescriptor EventDescriptor { get; }

        public Func<ISerializer, byte[]> Serialize { get; }
    }
}