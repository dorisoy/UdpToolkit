namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Network.Contracts;

    public interface IHostBuilder
    {
        public IHostBuilder ConfigureNetwork(
            Action<NetworkSettings> configurator);

        IHostBuilder ConfigureHost(
            Action<HostSettings> configurator);

        IHostBuilder ConfigureHostClient(
            Action<HostClientSettings> configurator);

        IHost Build();
    }
}