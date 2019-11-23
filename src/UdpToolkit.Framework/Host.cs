using UdpToolkit.Core;


namespace UdpToolkit.Framework
{
    public static class Host
    {
        public static IServerBuilder CreateServerBuilder()
        {
            return new ServerBuilder(
                serverSettings: new ServerSettings(), 
                containerBuilder: new ContainerBuilder());
        }
    }
}