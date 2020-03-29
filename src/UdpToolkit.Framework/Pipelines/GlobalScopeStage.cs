namespace UdpToolkit.Framework.Pipelines
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Peers;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Utils;

    public sealed class GlobalScopeStage : StageBase
    {
        private const byte GlobalScopeId = 0;
        private readonly IPeerScopeTracker _peerScopeTracker;
        private readonly IDateTimeProvider _dateTimeProvider;

        public GlobalScopeStage(
            IPeerScopeTracker peerScopeTracker,
            IDateTimeProvider dateTimeProvider)
        {
            _peerScopeTracker = peerScopeTracker;
            _dateTimeProvider = dateTimeProvider;
        }

        public override async Task ExecuteAsync(CallContext callContext)
        {
            var peer = callContext.PeerIPs.Single();
            var scopeId = callContext.ScopeId;

            var peerScope = _peerScopeTracker.GetOrAddScope(
                scopeId: scopeId,
                peerScope: new PeerScope(
                    scopeId: GlobalScopeId,
                    dateTimeProvider: new DateTimeProvider(),
                    cacheEntryTtl: Timeout.InfiniteTimeSpan,
                    scanFrequency: TimeSpan.MaxValue));

            var now = _dateTimeProvider.UtcNow();
            var peerIp = callContext.PeerIPs.Single();

            peerScope.AddPeer(peer: new Peer(
                id: peerIp.ToString(),
                ipEndPoint: peerIp,
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: now,
                createdAt: now));

            await ExecuteNext(callContext)
                .ConfigureAwait(false);
        }
    }
}
