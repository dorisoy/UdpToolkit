namespace UdpToolkit.Framework.Hubs
{
    public abstract class HubBase
    {
        protected IBroadcastResult Broadcast(object result)
        {
            return new BroadcastResult(result);
        }
    }
}
