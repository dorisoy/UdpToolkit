namespace UdpToolkit
{
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Network.Contracts;

    public static class UdpHost
    {
        public static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder(
                hostSettings: new HostSettings(),
                hostClientSettings: new HostClientSettings(),
                networkSettings: new NetworkSettings());
        }
    }
}
