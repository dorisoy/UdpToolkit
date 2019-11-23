using System;

namespace UdpToolkit.Core
{
    public interface IServerBuilder
    {
        IServerBuilder Configure(Action<ServerSettings> configurator);
        IServerBuilder ConfigureServices(Action<IContainerBuilder> configurator);
        IServer Build();
    }
}