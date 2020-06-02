namespace UdpToolkit.Framework.Client.Core
{
    using UdpToolkit.Core;

    public interface IClientHost : IHost
    {
        IEventProducerFactory GetEventProducerFactory();

        IEventConsumerFactory GetEventConsumerFactory();
    }
}