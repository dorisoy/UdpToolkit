namespace UdpToolkit.Framework.Server.Core
{
    using System.Threading.Tasks;

    public interface IPeerProxy
    {
        Task SendAsync<TEvent>(TEvent @event, HubContext hubContext);
    }
}
