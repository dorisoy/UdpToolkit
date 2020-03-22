namespace UdpToolkit.Framework.Hubs
{
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Queues;

    public delegate Task HubRpc(
        HubContext hubContext,
        ISerializer serializer,
        IPeerTracker peerTracker,
        IAsyncQueue<OutputUdpPacket> eventProducer,
        object[] ctorArguments,
        object[] methodArguments);
}