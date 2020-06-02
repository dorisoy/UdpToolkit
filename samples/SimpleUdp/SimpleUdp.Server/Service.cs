namespace SimpleUdp.Server
{
    using System.Threading.Tasks;

    public sealed class Service : IService
    {
        public Task ProcessAsync()
        {
            return Task.Delay(50);
        }
    }
}