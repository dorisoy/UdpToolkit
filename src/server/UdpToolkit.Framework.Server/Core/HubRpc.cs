namespace UdpToolkit.Framework.Server.Core
{
    using System.Threading.Tasks;

    public delegate Task HubRpc(
        IRoomManager roomManager,
        IHubClients hubClients,
        HubContext hubContext,
        object[] ctorArguments,
        object[] methodArguments);
}