namespace UdpToolkit.Framework.Pipelines
{
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;

    public abstract class StageBase : IStage
    {
        private IStage _next;

        public abstract Task ExecuteAsync(CallContext callContext);

        public IStage AppendStage(IStage next)
        {
            _next = next;

            return next;
        }

        public Task ExecuteNext(CallContext callContext)
        {
            if (_next != null)
            {
                return _next.ExecuteAsync(callContext);
            }

            return Task.CompletedTask;
        }
    }
}