namespace UdpToolkit.Core
{
    public interface IEventProducerFactory
    {
        IEventProducer<TEvent> Create<TEvent>(byte scopeId);
    }
}