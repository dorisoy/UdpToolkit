namespace UdpToolkit.Core.ProtocolEvents
{
    public abstract class ProtocolEvent<TEvent>
        where TEvent : ProtocolEvent<TEvent>, new()
    {
        public static byte[] Serialize(TEvent @event)
        {
            return @event.SerializeInternal(@event: @event);
        }

        public static TEvent Deserialize(byte[] bytes)
        {
            return new TEvent().DeserializeInternal(bytes);
        }

        protected abstract byte[] SerializeInternal(TEvent @event);

        protected abstract TEvent DeserializeInternal(byte[] bytes);
    }
}