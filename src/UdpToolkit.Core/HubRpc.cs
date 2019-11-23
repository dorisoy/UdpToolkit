using System.Threading.Tasks;

namespace UdpToolkit.Core
{
    public delegate Task HubRpc(
        HubContext hubContext,
        ISerializer serializer,
        IPeerTracker peerTracker,
        IUdpSenderProxy udpSenderProxy,
        object[] ctorArguments,
        object[] methodArguments);
}