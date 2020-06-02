namespace UdpToolkit.Framework.Client.Core
{
    using UdpToolkit.Network.Peers;

    public interface IServerSelector
    {
        Peer GetServer();
    }
}