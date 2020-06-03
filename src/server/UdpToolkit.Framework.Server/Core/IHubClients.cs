namespace UdpToolkit.Framework.Server.Core
{
    using System;

    public interface IHubClients
    {
        IPeerProxy All();

        IPeerProxy AllExcept(Guid peerId);

        IPeerProxy Room(byte roomId);

        IPeerProxy RoomExcept(byte roomId, Guid peerId);
    }
}
