namespace UdpToolkit.Framework.Server.Di.Autofac
{
    using System.Collections.Generic;
    using System.Linq;
    using global::Autofac;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Framework.Server.Pipelines;

    public class AutofacPipelineBuilder : IPipelineBuilder
    {
        private readonly Queue<IStage> _queue = new Queue<IStage>();
        private readonly IComponentContext _componentContext;

        public AutofacPipelineBuilder(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public IPipelineBuilder Append<TStage>()
            where TStage : IStage
        {
            var stage = _componentContext.Resolve<TStage>();

            _queue.Enqueue(stage);

            return this;
        }

        public IPipeline Build()
        {
            var stages = _queue.ToList();

            return new Pipeline(stages: stages);
        }
    }
}