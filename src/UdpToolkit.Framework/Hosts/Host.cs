using UdpToolkit.Core;
using UdpToolkit.Framework.Di;
using UdpToolkit.Framework.Hosts.Client;
using UdpToolkit.Framework.Hosts.Server;


namespace UdpToolkit.Framework.Hosts
{
    public static class Host
    {
        public static IServerHostBuilder CreateServerBuilder()
        {
            return new ServerHostBuilder(
                serverSettings: new ServerSettings(), 
                containerBuilder: new ContainerBuilder());
        }

        public static IClientHostBuilder CreateClientBuilder()
        {
            return new ClientHostHostBuilder(
                clientSettings: new ClientSettings());
        }
    }
}