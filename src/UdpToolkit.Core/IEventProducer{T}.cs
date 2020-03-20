namespace UdpToolkit.Core
{
    public interface IEventProducer<in TEvent>
    {
        void Produce(TEvent @event);
    }
}