namespace UdpToolkit.Core
{
    using System;

    public interface IHostBuilder
    {
        IHostBuilder ConfigureHost(Action<HostSettings> configurator);

        IHostBuilder ConfigureServerHostClient(Action<ServerHostClientSettings> configurator);

        IHost Build();
    }
}