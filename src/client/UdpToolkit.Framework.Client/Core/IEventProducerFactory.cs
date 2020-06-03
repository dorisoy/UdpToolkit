namespace UdpToolkit.Framework.Client.Core
{
    public interface IEventProducerFactory
    {
        IEventProducer<TEvent> Create<TEvent>(byte roomId);
    }
}