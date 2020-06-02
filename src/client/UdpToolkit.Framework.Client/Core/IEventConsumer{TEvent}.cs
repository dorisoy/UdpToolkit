namespace UdpToolkit.Framework.Client.Core
{
    using System.Collections.Generic;

    public interface IEventConsumer<out TEvent> : IEventConsumer
    {
        IEnumerable<TEvent> Consume();
    }
}