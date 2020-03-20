using UdpToolkit.Network.Peers;

namespace UdpToolkit.Framework.Hosts
{
    public interface IServerSelector
    {
        Peer GetServer();
    }
}