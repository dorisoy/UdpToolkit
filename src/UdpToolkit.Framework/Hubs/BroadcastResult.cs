namespace UdpToolkit.Framework.Hubs
{
    public sealed class BroadcastResult : IBroadcastResult
    {
        public BroadcastResult(object result)
        {
            Result = result;
        }

        public object Result { get; }
    }
}