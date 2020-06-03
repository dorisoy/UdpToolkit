namespace UdpToolkit.Framework.Server.Hubs
{
    using UdpToolkit.Framework.Server.Core;

    public abstract class HubBase
    {
        public IRoomManager Rooms { get; set; }

        public IHubClients Clients { get; set; }

        public HubContext HubContext { get; set; }
    }
}
