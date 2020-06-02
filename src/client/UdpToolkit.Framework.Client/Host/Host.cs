namespace UdpToolkit.Framework.Client.Host
{
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Client.Core;

    public static class Host
    {
        public static IClientHostBuilder CreateClientBuilder()
        {
            return new ClientHostHostBuilder(
                settings: new ClientSettings());
        }
    }
}