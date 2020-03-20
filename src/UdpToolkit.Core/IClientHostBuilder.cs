using System;

namespace UdpToolkit.Core
{
    public interface IClientHostBuilder
    {
        IClientHostBuilder Configure(Action<ClientSettings> configurator);

        IClientHost Build();
    }
}