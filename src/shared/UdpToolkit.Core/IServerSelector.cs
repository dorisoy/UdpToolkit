namespace UdpToolkit.Core
{
    using System.Net;

    public interface IServerSelector
    {
        IPeer GetServer();

        bool IsServerIp(IPEndPoint ipEndPoint);
    }
}