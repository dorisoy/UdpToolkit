using System.Collections.Generic;

namespace UdpToolkit.Core
{
    public interface IEventConsumer<out TEvent> : IEventConsumer
    {
        IEnumerable<TEvent> Consume();
    }
}