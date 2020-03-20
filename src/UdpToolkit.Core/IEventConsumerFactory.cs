namespace UdpToolkit.Core
{
    public interface IEventConsumerFactory
    {
        IEventConsumer<TEvent> Create<TEvent>();
    }
}