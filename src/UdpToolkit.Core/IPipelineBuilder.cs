namespace UdpToolkit.Core
{
    public interface IPipelineBuilder
    {
        IPipelineBuilder Append<TStage>()
            where TStage : IStage;

        IPipeline Build();
    }
}