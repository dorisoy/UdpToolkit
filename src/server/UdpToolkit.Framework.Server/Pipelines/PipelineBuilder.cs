namespace UdpToolkit.Framework.Server.Pipelines
{
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Framework.Server.Core;

    public class PipelineBuilder : IPipelineBuilder
    {
        private readonly Queue<IStage> _queue = new Queue<IStage>();
        private readonly IContainer _container;

        public PipelineBuilder(IContainer container)
        {
            _container = container;
        }

        public IPipelineBuilder Append<TStage>()
            where TStage : IStage
        {
            var stage = _container.GetInstance<TStage>();

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