namespace UdpToolkit.Framework.Events
{
    using System;
    using UdpToolkit.Core;

    public readonly struct ProducedEvent
    {
        public ProducedEvent(
            byte scopeId,
            EventDescriptor eventDescriptor,
            Func<ISerializer, byte[]> serialize)
        {
            ScopeId = scopeId;
            EventDescriptor = eventDescriptor;
            Serialize = serialize;
        }

        public byte ScopeId { get; }

        public EventDescriptor EventDescriptor { get; }

        public Func<ISerializer, byte[]> Serialize { get; }
    }
}