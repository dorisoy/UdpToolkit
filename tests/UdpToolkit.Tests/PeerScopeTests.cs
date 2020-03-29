namespace UdpToolkit.Tests
{
    using System;
    using System.Net;
    using UdpToolkit.Framework.Peers;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Tests.Fakes;
    using UdpToolkit.Tests.Utils;
    using UdpToolkit.Utils;
    using Xunit;

    public class PeerScopeTests
    {
        [Fact]
        public void PeerScope_CreateNew_IsEmpty()
        {
            var dateTimeProvider = new DateTimeProvider();
            var scanFrequency = TimeSpan.FromMinutes(0);

            var scope = new PeerScope(
                scopeId: Gen.RandomByte(),
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: Gen.RandomTimeSpanFromMinutes(),
                scanFrequency: scanFrequency);

            var peers = scope.GetPeers();

            Assert.Empty(peers);
        }

        [Fact]
        public void PeerScope_AddPeer_Added()
        {
            var dateTimeProvider = new DateTimeProvider();
            var now = dateTimeProvider.UtcNow();

            var peerTtl = TimeSpan.FromMinutes(10);
            var scanFrequency = TimeSpan.FromMinutes(0);

            var scope = new PeerScope(
                scopeId: Gen.RandomByte(),
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: peerTtl,
                scanFrequency: scanFrequency);

            var remoteIp = new IPEndPoint(
                address: IPAddress.Loopback,
                port: Gen.RandomPort());

            var peer = new Peer(
                id: Gen.RandomString(),
                ipEndPoint: remoteIp,
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: now,
                createdAt: now);

            scope.AddPeer(peer: peer);

            var peers = scope.GetPeers();

            Assert.Single(peers);
        }

        [Fact]
        public void PeerScope_AddPeerWithTtl_ScopeExpired()
        {
            var dateTimeProvider = new DateTimeProvider();
            var now = dateTimeProvider.UtcNow();

            var peerScopeTtl = TimeSpan.FromMinutes(-1);
            var peerTtl = TimeSpan.FromMinutes(-1);

            var scanFrequency = TimeSpan.FromMinutes(0);

            var scope = new PeerScope(
                scopeId: Gen.RandomByte(),
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: peerScopeTtl,
                scanFrequency: scanFrequency);

            var remoteIp = new IPEndPoint(
                address: IPAddress.Loopback,
                port: Gen.RandomPort());

            var peer = new Peer(
                id: Gen.RandomString(),
                ipEndPoint: remoteIp,
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: now,
                createdAt: now);

            scope.AddPeer(peer: peer);

            var expired = scope.IsExpired(now, peerScopeTtl);

            Assert.True(expired);
        }

        [Fact]
        public void PeerScope_AddPeer_PeerExpired()
        {
            var createdAt = "1/25/2020 1:30:30 PM +00:00";

            var dateTimeProvider = new FakeDateTimeProvider(createdAt);
            var now = dateTimeProvider.UtcNow();

            var peerTtl = TimeSpan.FromMinutes(1);
            var scanFrequency = TimeSpan.FromMinutes(0);

            var scope = new PeerScope(
                scopeId: Gen.RandomByte(),
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: peerTtl,
                scanFrequency: scanFrequency);

            var ipEndPoint = new IPEndPoint(
                address: IPAddress.Loopback,
                port: Gen.RandomPort());

            var peer = new Peer(
                id: Gen.RandomString(),
                ipEndPoint: ipEndPoint,
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: now,
                createdAt: now);

            scope.AddPeer(peer: peer);

            peer.UpdateLastActivity(now - TimeSpan.FromMinutes(5));

            var result = scope.TryGetPeer(peer.Id, out var _);

            Assert.False(result);
        }
    }
}