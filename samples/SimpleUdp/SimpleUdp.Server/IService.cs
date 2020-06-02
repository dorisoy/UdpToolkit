namespace SimpleUdp.Server
{
    using System.Threading.Tasks;

    public interface IService
    {
        Task ProcessAsync();
    }
}