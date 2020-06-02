namespace UdpToolkit.Framework.Server.Hubs
{
    using UdpToolkit.Framework.Server.Core;

    public abstract class HubBase
    {
        protected IBroadcastResult Broadcast(object result)
        {
            return new BroadcastResult(result);
        }
    }
}
