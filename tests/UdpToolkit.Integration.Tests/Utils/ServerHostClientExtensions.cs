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

            host.OnProtocolInternal<Connected>(
                handler: (guid, @event) =>
                {
                    manualResetEvent.Set();
                },
                hookId: (byte)PacketType.Connected);
            manualResetEvent.WaitOne();
        }
    }
}