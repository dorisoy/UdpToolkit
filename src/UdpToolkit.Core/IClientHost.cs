namespace UdpToolkit.Core
{
    public interface IClientHost : IHost
    {
        IEventProducerFactory GetEventProducerFactory();

        IEventConsumerFactory GetEventConsumerFactory();
    }
}