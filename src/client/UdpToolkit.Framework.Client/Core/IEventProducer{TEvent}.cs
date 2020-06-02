namespace UdpToolkit.Framework.Client.Core
{
    public interface IEventProducer<in TEvent>
    {
        void Produce(TEvent @event);
    }
}