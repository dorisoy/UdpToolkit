namespace UdpToolkit
{
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;

    public static class UdpHost
    {
        public static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder(
                hostClientSettings: new HostClientSettings(),
                networkSettings: new NetworkSettings());
        }
    }
}
