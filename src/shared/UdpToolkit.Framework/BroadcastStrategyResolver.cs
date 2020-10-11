namespace UdpToolkit.Framework
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class BroadcastStrategyResolver : IBroadcastStrategyResolver
    {
        private readonly IReadOnlyDictionary<BroadcastType, IBroadcastStrategy> _broadcastStrategies;

        public BroadcastStrategyResolver(
            IEnumerable<IBroadcastStrategy> broadcastStrategies)
        {
            _broadcastStrategies = broadcastStrategies
                .ToDictionary(
                    keySelector: strategy => strategy.Type,
                    elementSelector: strategy => strategy);
        }

        public IBroadcastStrategy Resolve(BroadcastType broadcastType) => _broadcastStrategies[broadcastType];
    }
}