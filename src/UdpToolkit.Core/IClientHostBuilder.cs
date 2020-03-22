namespace UdpToolkit.Core
{
    using System;

    public interface IClientHostBuilder
    {
        IClientHostBuilder Configure(Action<ClientSettings> configurator);

        IClientHost Build();
    }
}