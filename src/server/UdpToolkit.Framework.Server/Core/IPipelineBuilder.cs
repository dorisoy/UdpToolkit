namespace UdpToolkit.Framework.Server.Core
{
    public interface IPipelineBuilder
    {
        IPipelineBuilder Append<TStage>()
            where TStage : IStage;

        IPipeline Build();
    }
}