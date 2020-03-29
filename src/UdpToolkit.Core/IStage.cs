namespace UdpToolkit.Core
{
    using System.Threading.Tasks;

    public interface IStage
    {
        IStage AppendStage(IStage next);

        Task ExecuteNext(CallContext callContext);

        Task ExecuteAsync(CallContext callContext);
    }
}