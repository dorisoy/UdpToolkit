namespace UdpToolkit.Framework.Server.Pipelines
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Server.Core;

    public sealed class Pipeline : IPipeline
    {
        private readonly IReadOnlyCollection<IStage> _stages;

        public Pipeline(IReadOnlyCollection<IStage> stages)
        {
            _stages = stages;
        }

        public async Task ExecuteAsync(CallContext callContext)
        {
            foreach (var stage in _stages)
            {
                await stage
                    .ExecuteAsync(callContext: callContext)
                    .ConfigureAwait(false);
            }
        }
    }
}