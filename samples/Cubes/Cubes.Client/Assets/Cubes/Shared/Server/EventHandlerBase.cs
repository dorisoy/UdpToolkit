namespace Cubes.Shared.Server
{
    using UdpToolkit.Core;

    public abstract class EventHandlerBase
    {
        protected EventHandlerBase(
            IHost host,
            INetworkThreadDispatcher dispatcher)
        {
            Host = host;
            Dispatcher = dispatcher;
        }

        protected IHost Host { get; }

        protected INetworkThreadDispatcher Dispatcher { get; }
    }
}