namespace UdpToolkit.Integration.Tests.Utils
{
    using UdpToolkit;
    using UdpToolkit.Core;
    using UdpToolkit.Serialization.MsgPack;

    public static class HostFactory
    {
        public static IHost CreateServerHost(
            int[] inputPorts,
            int[] outputPorts)
        {
            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(settings =>
                {
                    settings.Host = "0.0.0.0";
                    settings.HostPorts = inputPorts;
                    settings.Workers = 2;
                    settings.Serializer = new Serializer();
                })
                .Build();
        }

        public static IHost CreateClientHost(
            int[] inputPorts,
            int[] outputPorts,
            int[] serverInputPorts)
        {
            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(settings =>
                {
                    settings.Host = "0.0.0.0";
                    settings.HostPorts = inputPorts;
                    settings.Workers = 2;
                    settings.Serializer = new Serializer();
                })
                .ConfigureHostClient((settings) =>
                {
                    settings.ServerHost = "0.0.0.0";
                    settings.ServerPorts = serverInputPorts;
                })
                .Build();
        }
    }
}