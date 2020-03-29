namespace UdpToolkit.Framework.Pipelines
{
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Core;

    public class PipelineBuilder : IPipelineBuilder
    {
        private readonly Queue<IStage> _queue = new Queue<IStage>();
        private readonly IRegistrationContext _registrationContext;

        public PipelineBuilder(IRegistrationContext registrationContext)
        {
            _registrationContext = registrationContext;
        }

        public IPipelineBuilder Append<TStage>()
            where TStage : IStage
        {
            var stage = _registrationContext.GetInstance<TStage>();

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