namespace UdpToolkit.Framework
{
    using System.Net;

    public interface IRawServerSelector
    {
        Peer GetServer();
    }
}