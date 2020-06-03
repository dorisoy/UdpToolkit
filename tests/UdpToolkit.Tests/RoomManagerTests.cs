namespace UdpToolkit.Tests
{
    using System;
    using System.Net;
    using UdpToolkit.Framework.Server.Peers;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Tests.Utils;
    using UdpToolkit.Utils;
    using Xunit;

    public class RoomManagerTests
    {
        [Fact]
        public void RoomManager_JoinOrCreate_RoomCreated()
        {
            var peerManager = new PeerManager(new DateTimeProvider());
            var roomManager = new RoomManager(peerManager: peerManager);

            var room = new Room(
                roomId: Gen.RandomByte());

            var peer = new Peer(
                peerId: Guid.NewGuid(),
                ipEndPoint: IPEndPoint.Parse("0.0.0.0"),
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: DateTimeOffset.Now,
                createdAt: DateTimeOffset.Now);

            roomManager.JoinOrCreate(
                roomId: room.RoomId,
                peerId: peer.PeerId);

            Assert.Equal(1, actual: room.Size);
        }

        [Fact]
        public void RoomManager_JoinOrCreate_RoomExpired()
        {
            var peer = new Peer(
                peerId: Guid.NewGuid(),
                ipEndPoint: IPEndPoint.Parse("0.0.0.0"),
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: DateTimeOffset.Now,
                createdAt: DateTimeOffset.Now);

            var peerManager = new PeerManager(new DateTimeProvider());
            var roomManager = new RoomManager(peerManager: peerManager);

            var room = new Room(
                roomId: Gen.RandomByte());

            roomManager.JoinOrCreate(
                roomId: room.RoomId,
                peerId: peer.PeerId);

            throw new NotImplementedException();
        }

        [Fact]
        public void RoomManager_JoinOrCreate_RoomNotExpired()
        {
            var peerManager = new PeerManager(new DateTimeProvider());
            var roomManager = new RoomManager(peerManager: peerManager);

            var room = new Room(
                roomId: Gen.RandomByte());

            roomManager.JoinOrCreate(
                roomId: room.RoomId,
                peerId: Guid.NewGuid());

            throw new NotImplementedException();
        }

        [Fact]
        public void RoomManager_JoinOrCreate_RoomNotExpiredNever()
        {
            var room = new Room(
                roomId: Gen.RandomByte());

            var peerManager = new PeerManager(new DateTimeProvider());
            var roomManager = new RoomManager(peerManager: peerManager);

            roomManager.JoinOrCreate(
                roomId: room.RoomId,
                peerId: Guid.NewGuid());

            throw new NotImplementedException();
        }
    }
}
