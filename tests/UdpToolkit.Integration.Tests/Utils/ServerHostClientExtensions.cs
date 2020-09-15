namespace UdpToolkit.Integration.Tests.Utils
{
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Framework;
    using UdpToolkit.Network.Channels;

    public static class ServerHostClientExtensions
    {
        public static void ConnectInternal(this IHost host)
        {
            var serverHostClient = host.ServerHostClient;
            serverHostClient.Connect();
            var manualResetEvent = new ManualResetEvent(initialState: false);

            host.On<Connected>(
                handler: (guid, @event) =>
                {
                    manualResetEvent.Set();
                },
                packetType: PacketType.Connected);

            manualResetEvent.WaitOne(5000);
        }
    }
}