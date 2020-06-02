namespace UdpToolkit.Framework.Server.Pipelines
{
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Framework.Server.Host;
    using UdpToolkit.Framework.Server.Peers;
    using UdpToolkit.Framework.Server.Rpcs;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;

    public sealed class ProcessStage : StageBase
    {
        private readonly ILogger _logger = Log.ForContext<ServerHost>();

        private readonly ICtorArgumentsResolver _ctorArgumentsResolver;
        private readonly IRpcProvider _rpcProvider;
        private readonly ISerializer _serializer;
        private readonly IPeerScopeTracker _peerScopeTracker;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;

        public ProcessStage(
            IRpcProvider rpcProvider,
            ISerializer serializer,
            ICtorArgumentsResolver ctorArgumentsResolver,
            IPeerScopeTracker peerScopeTracker,
            IAsyncQueue<NetworkPacket> outputQueue)
        {
            _rpcProvider = rpcProvider;
            _serializer = serializer;
            _ctorArgumentsResolver = ctorArgumentsResolver;
            _peerScopeTracker = peerScopeTracker;
            _outputQueue = outputQueue;
        }

        public override async Task ExecuteAsync(CallContext callContext)
        {
            var key = new RpcDescriptorId(hubId: callContext.HubId, rpcId: callContext.RpcId);
            if (!_rpcProvider.TryProvide(key, out var rpcDescriptor))
            {
                _logger.Warning("Rpc not found by rpcDescriptor: {@rpcDescriptor}", rpcDescriptor);

                return;
            }

            if (rpcDescriptor.ParametersTypes.Count > 1)
            {
                _logger.Warning("Rpc not support more than one argument");

                return;
            }

            var @event = rpcDescriptor.ParametersTypes
                .Select(type => _serializer.Deserialize(type, callContext.Payload))
                .ToArray();

            var result = await rpcDescriptor
                .HubRpc(
                    ctorArguments: _ctorArgumentsResolver
                        .GetInstances(rpcDescriptor.CtorArguments)
                        .ToArray(),
                    methodArguments: @event)
                .ConfigureAwait(false);

            var bytes = _serializer.Serialize(result.Result);

            if (!_peerScopeTracker.TryGetScope(callContext.ScopeId, out var scope))
            {
                // TODO log warning
                return;
            }

            var peers = scope.GetPeers();

            var packet = new NetworkPacket(
                payload: bytes,
                peers: peers,
                udpMode: callContext.UdpMode.Map(),
                frameworkHeader: new FrameworkHeader(
                    hubId: callContext.HubId,
                    rpcId: callContext.RpcId,
                    scopeId: callContext.ScopeId));

            _outputQueue.Produce(@event: packet);
        }
    }
}