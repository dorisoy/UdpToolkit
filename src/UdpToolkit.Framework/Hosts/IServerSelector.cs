namespace UdpToolkit.Framework.Hosts
{
    using UdpToolkit.Network.Peers;

    public interface IServerSelector
    {
        Peer GetServer();
    }
}