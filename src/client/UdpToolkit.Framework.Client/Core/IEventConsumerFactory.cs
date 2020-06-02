namespace UdpToolkit.Framework.Client.Core
{
    public interface IEventConsumerFactory
    {
        IEventConsumer<TEvent> Create<TEvent>();
    }
}