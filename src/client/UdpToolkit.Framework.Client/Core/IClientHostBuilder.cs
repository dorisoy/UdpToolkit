namespace UdpToolkit.Framework.Client.Core
{
    using System;
    using UdpToolkit.Core;

    public interface IClientHostBuilder
    {
        IClientHostBuilder Configure(Action<ClientSettings> configurator);

        IClientHost Build();
    }
}