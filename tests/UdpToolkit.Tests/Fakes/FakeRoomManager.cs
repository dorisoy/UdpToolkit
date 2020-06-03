namespace UdpToolkit.Tests.Fakes
{
    using System;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Framework.Server.Peers;

    public class FakeRoomManager : IRoomManager
    {
        public void JoinOrCreate(ushort roomId, Guid peerId)
        {
            throw new NotImplementedException();
        }

        public void JoinOrCreate(ushort roomId, Guid peerId, int limit)
        {
            throw new NotImplementedException();
        }

        public IRoom GetRoom(ushort roomId)
        {
            throw new NotImplementedException();
        }

        public void Leave(ushort roomId, Guid peerId)
        {
            throw new NotImplementedException();
        }
    }
}