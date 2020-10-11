namespace UdpToolkit.Framework
{
    public interface IBroadcastStrategyResolver
    {
        IBroadcastStrategy Resolve(
            BroadcastType broadcastType);
    }
}