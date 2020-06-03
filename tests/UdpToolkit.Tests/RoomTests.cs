namespace UdpToolkit.Tests
{
    using System;
    using System.Net;
    using UdpToolkit.Framework.Server.Peers;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Tests.Fakes;
    using UdpToolkit.Tests.Utils;
    using UdpToolkit.Utils;
    using Xunit;

    public class RoomTests
    {
        [Fact]
        public void Room_CreateNew_IsEmpty()
        {
            var room = new Room(
                roomId: Gen.RandomByte());

            var peers = room.GetPeers();

            Assert.Empty(peers);
        }

        [Fact]
        public void Room_AddPeer_Added()
        {
            var dateTimeProvider = new DateTimeProvider();
            var now = dateTimeProvider.UtcNow();

            var room = new Room(
                roomId: Gen.RandomByte());

            var remoteIp = new IPEndPoint(
                address: IPAddress.Loopback,
                port: Gen.RandomPort());

            var peer = new Peer(
                peerId: Guid.NewGuid(),
                ipEndPoint: remoteIp,
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: now,
                createdAt: now);

            room.AddPeer(peer: peer);

            var peers = room.GetPeers();

            Assert.Single(peers);
        }

        [Fact]
        public void Room_AddPeerWithTtl_RoomExpired()
        {
            var dateTimeProvider = new DateTimeProvider();
            var now = dateTimeProvider.UtcNow();

            var room = new Room(
                roomId: Gen.RandomByte());

            var remoteIp = new IPEndPoint(
                address: IPAddress.Loopback,
                port: Gen.RandomPort());

            var peer = new Peer(
                peerId: Guid.NewGuid(),
                ipEndPoint: remoteIp,
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: now,
                createdAt: now);

            room.AddPeer(peer: peer);

            throw new NotImplementedException();
        }

        [Fact]
        public void Room_AddPeer_PeerExpired()
        {
            var createdAt = "1/25/2020 1:30:30 PM +00:00";

            var dateTimeProvider = new FakeDateTimeProvider(createdAt);
            var now = dateTimeProvider.UtcNow();

            var room = new Room(
                roomId: Gen.RandomByte());

            var ipEndPoint = new IPEndPoint(
                address: IPAddress.Loopback,
                port: Gen.RandomPort());

            var peer = new Peer(
                peerId: Guid.NewGuid(),
                ipEndPoint: ipEndPoint,
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: now,
                createdAt: now);

            room.AddPeer(peer: peer);

            peer.UpdateLastActivity(now - TimeSpan.FromMinutes(5));

            var p = room.GetPeer(peer.PeerId);

            Assert.NotNull(p);
        }
    }
}