namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class TimersPool : ITimersPool
    {
        private readonly ILogger _logger = Log.ForContext<Host>();
        private readonly ConcurrentDictionary<Guid, Lazy<Timer>> _timers;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly IProtocolSubscriptionManager _protocolSubscriptionManager;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IRawPeerManager _rawPeerManager;
        private readonly IRawRoomManager _rawRoomManager;

        public TimersPool(
            IAsyncQueue<NetworkPacket> outputQueue,
            IProtocolSubscriptionManager protocolSubscriptionManager,
            ISubscriptionManager subscriptionManager,
            IRawPeerManager peerManager,
            IRawRoomManager roomManager)
        {
            _timers = new ConcurrentDictionary<Guid, Lazy<Timer>>();
            _outputQueue = outputQueue;
            _protocolSubscriptionManager = protocolSubscriptionManager;
            _subscriptionManager = subscriptionManager;
            _rawPeerManager = peerManager;
            _rawRoomManager = roomManager;
        }

        public void EnableResend(
            IPeer peer)
        {
            _logger.Information(nameof(EnableResend));

            var lazyTimer = _timers.GetOrAdd(
                key: peer.PeerId,
                valueFactory: (key) => new Lazy<Timer>(
                    valueFactory: () => new Timer(
                    callback: (state) => ResendLostPackets(peer as Peer),
                    state: null,
                    dueTime: TimeSpan.FromMilliseconds(2000),
                    period: TimeSpan.FromMilliseconds(2000))));

            _ = lazyTimer.Value;
        }

        public bool DisableResend(
            Guid peerId)
        {
            _logger.Information(nameof(DisableResend));
            var timerRemoved = _timers.Remove(peerId, out var lazyTimer);
            if (!timerRemoved)
            {
                return false;
            }

            lazyTimer.Value.Dispose();
            return true;
        }

        public void Dispose()
        {
            foreach (var timer in _timers)
            {
                timer.Value?.Value.Dispose();
            }

            _outputQueue?.Dispose();
        }

        private void ResendLostPackets(Peer peer)
        {
            _logger.Information(nameof(ResendLostPackets));

            if (peer.IsExpired())
            {
                _logger.Debug($"Peer {peer.PeerId} - removed by inactivity timeout!");

                _rawRoomManager.Remove(
                    roomId: peer.GetRoomId(),
                    peer: peer);

                _rawPeerManager.Remove(peer);

                if (_timers.Remove(peer.PeerId, out var timer))
                {
                    timer.Value.Change(Timeout.Infinite, Timeout.Infinite);
                    timer.Value.Dispose();
                }

                return;
            }

            foreach (var channel in peer.GetChannels())
            {
                var packets = channel
                    .ToResend();

                var ps = packets.ToArray();
                foreach (var packet in ps)
                {
                    if (packet.IsExpired())
                    {
                        if (packet.IsProtocolEvent)
                        {
                            _protocolSubscriptionManager
                                .GetProtocolSubscription(packet.HookId)
                                ?.OnTimeout(packet.PeerId);
                        }
                        else
                        {
                            _subscriptionManager
                                .GetSubscription(packet.HookId)
                                ?.OnTimeout(peer.PeerId);
                        }
                    }
                    else
                    {
                        _outputQueue.Produce(packet);
                    }
                }
            }
        }
    }
}