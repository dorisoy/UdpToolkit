namespace UdpToolkit.Framework.Server.Core
{
    using System.Threading.Tasks;

    public delegate Task<IRpcResult> HubRpc(
        object[] ctorArguments,
        object[] methodArguments);
}