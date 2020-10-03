namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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

        public TimersPool(
            ConcurrentDictionary<Guid, Lazy<Timer>> timers,
            IAsyncQueue<NetworkPacket> outputQueue)
        {
            _timers = timers;
            _outputQueue = outputQueue;
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
                    dueTime: TimeSpan.FromMilliseconds(0),
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
            foreach (var channel in peer.GetChannels())
            {
                var packets = channel
                    .ToResend();

                foreach (var packet in packets)
                {
                    _outputQueue.Produce(packet);
                }
            }
        }
    }
}