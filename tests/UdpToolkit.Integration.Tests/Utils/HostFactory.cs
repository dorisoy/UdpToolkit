namespace UdpToolkit.Integration.Tests.Utils
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Framework;
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
                    settings.InputPorts = inputPorts;
                    settings.OutputPorts = outputPorts;
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
                    settings.InputPorts = inputPorts;
                    settings.OutputPorts = outputPorts;
                    settings.Workers = 2;
                    settings.Serializer = new Serializer();
                })
                .ConfigureServerHostClient((settings) =>
                {
                    settings.ServerHost = "0.0.0.0";
                    settings.ServerInputPorts = serverInputPorts;
                })
                .Build();
        }
    }
}