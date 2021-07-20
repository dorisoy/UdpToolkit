namespace UdpToolkit
{
    using UdpToolkit.Core;
    using UdpToolkit.Core.Settings;

    public static class UdpHost
    {
        public static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder(
                networkSettings: new NetworkSettings(),
                hostSettings: new HostSettings(),
                hostClientSettings: new HostClientSettings());
        }
    }
}
