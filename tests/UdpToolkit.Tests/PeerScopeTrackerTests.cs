namespace UdpToolkit.Tests
{
    using System;
    using System.Threading;
    using UdpToolkit.Framework.Peers;
    using UdpToolkit.Tests.Fakes;
    using UdpToolkit.Tests.Utils;
    using UdpToolkit.Utils;
    using Xunit;

    public class PeerScopeTrackerTests
    {
        [Fact]
        public void PeerScopeTracker_GetOrAddScope_ScopeAdded()
        {
            var scanFrequency = TimeSpan.FromMinutes(1);
            var cacheEntryTtl = TimeSpan.FromMinutes(10);

            var peerTracker = new PeerScopeTracker(
                dateTimeProvider: new DateTimeProvider(),
                cacheEntryTtl: cacheEntryTtl,
                scanFrequency: scanFrequency);

            var scope = new PeerScope(
                scopeId: Gen.RandomByte(),
                dateTimeProvider: new DateTimeProvider(),
                cacheEntryTtl: cacheEntryTtl,
                scanFrequency: scanFrequency);

            var peerScope = peerTracker.GetOrAddScope(
                scopeId: scope.ScopeId,
                peerScope: scope);

            Assert.Equal(expected: scope.ScopeId, actual: peerScope.ScopeId);
        }

        [Fact]
        public void PeerScopeTracker_GetOrAddScope_ScopeExpired()
        {
            var createdAt = "1/25/2020 1:30:30 PM +00:00";

            var peerScopeTtl = TimeSpan.FromMinutes(-1);
            var scanFrequency = TimeSpan.FromMinutes(0);

            var dateTimeProvider = new FakeDateTimeProvider(
                date: createdAt);

            var peerTracker = new PeerScopeTracker(
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: peerScopeTtl,
                scanFrequency: scanFrequency);

            var scope = new PeerScope(
                scopeId: Gen.RandomByte(),
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: Gen.RandomTimeSpanFromMinutes(),
                scanFrequency: scanFrequency);

            var peerScope = peerTracker.GetOrAddScope(
                scopeId: scope.ScopeId,
                scope);

            Assert.Null(peerScope);
        }

        [Fact]
        public void PeerScopeTracker_GetOrAddScope_ScopeNotExpired()
        {
            var createdAt = "1/25/2020 1:30:30 PM +00:00";

            var peerScopeTtl = TimeSpan.FromMinutes(1);
            var peerTtl = TimeSpan.FromMinutes(1);
            var scanFrequency = TimeSpan.FromMinutes(0);

            var dateTimeProvider = new FakeDateTimeProvider(
                date: createdAt);

            var peerTracker = new PeerScopeTracker(
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: peerScopeTtl,
                scanFrequency: scanFrequency);

            var peerScope = new PeerScope(
                scopeId: Gen.RandomByte(),
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: peerTtl,
                scanFrequency: scanFrequency);

            var result = peerTracker.GetOrAddScope(
                scopeId: peerScope.ScopeId,
                peerScope: peerScope);

            Assert.NotNull(result);
        }

        [Fact]
        public void PeerScopeTracker_GetOrAddScope_ScopeNotExpiredNever()
        {
            var createdAt = "1/25/2020 1:30:30 PM +00:00";
            var expiredAt = "1/25/2025 1:30:30 PM +00:00";

            var peerScopeTtl = Timeout.InfiniteTimeSpan;
            var peerTtl = Timeout.InfiniteTimeSpan;

            var scanFrequency = TimeSpan.FromMinutes(0);

            var scopeId = Gen.RandomByte();

            var dateTimeProvider = new FakeDateTimeProvider(createdAt);

            var scope = new PeerScope(
                scopeId: Gen.RandomByte(),
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: peerTtl,
                scanFrequency: scanFrequency);

            var peerScopeTracker = new PeerScopeTracker(
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: peerScopeTtl,
                scanFrequency: scanFrequency);

            peerScopeTracker.GetOrAddScope(
                scopeId: scopeId,
                peerScope: scope);

            dateTimeProvider.RewindDateTime(
                date: expiredAt);

            var result = peerScopeTracker.TryGetScope(
                scopeId: scopeId,
                scope: out var peerScope);

            Assert.True(result);
            Assert.NotNull(peerScope);
        }
    }
}
