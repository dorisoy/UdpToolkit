namespace UdpToolkit.Core
{
    using System.Threading.Tasks;

    public interface IPipeline
    {
        Task ExecuteAsync(CallContext callContext);
    }
}