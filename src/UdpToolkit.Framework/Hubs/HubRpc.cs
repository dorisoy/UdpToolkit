namespace UdpToolkit.Framework.Hubs
{
    using System.Threading.Tasks;

    public delegate Task<IRpcResult> HubRpc(
        object[] ctorArguments,
        object[] methodArguments);
}