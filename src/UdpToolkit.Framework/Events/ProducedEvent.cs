using System;
using UdpToolkit.Core;

namespace UdpToolkit.Framework.Events
{
    public readonly struct ProducedEvent
    {
        public byte ScopeId { get; }
        public EventDescriptor EventDescriptor { get; }
        public Func<ISerializer, byte[]> Serialize { get; }

        public ProducedEvent(
            byte scopeId,
            EventDescriptor eventDescriptor,
            Func<ISerializer, byte[]> serialize)
        {
            ScopeId = scopeId;
            EventDescriptor = eventDescriptor;
            Serialize = serialize;
        }
    }
}