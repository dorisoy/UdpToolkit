namespace UdpToolkit.Framework.Server.Hubs
{
    using UdpToolkit.Framework.Server.Core;

    public sealed class BroadcastResult : IBroadcastResult
    {
        public BroadcastResult(object result)
        {
            Result = result;
        }

        public object Result { get; }
    }
}