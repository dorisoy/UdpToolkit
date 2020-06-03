namespace UdpToolkit.Framework.Server.Pipelines
{
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Framework.Server.Host;

    public sealed class ProcessStage : StageBase
    {
        private readonly ILogger _logger = Log.ForContext<ServerHost>();

        private readonly ICtorArgumentsResolver _ctorArgumentsResolver;
        private readonly IRpcProvider _rpcProvider;
        private readonly ISerializer _serializer;
        private readonly IRoomManager _roomManager;
        private readonly IHubClients _hubClients;

        public ProcessStage(
            IRpcProvider rpcProvider,
            ISerializer serializer,
            ICtorArgumentsResolver ctorArgumentsResolver,
            IRoomManager roomManager,
            IHubClients hubClients)
        {
            _rpcProvider = rpcProvider;
            _serializer = serializer;
            _ctorArgumentsResolver = ctorArgumentsResolver;
            _roomManager = roomManager;
            _hubClients = hubClients;
        }

        public override async Task ExecuteAsync(CallContext callContext)
        {
            var key = new RpcDescriptorId(hubId: callContext.HubId, rpcId: callContext.RpcId);
            if (!_rpcProvider.TryProvide(key, out var rpcDescriptor))
            {
                _logger.Warning("Rpc not found by rpcDescriptor: {@rpcDescriptor}", rpcDescriptor);

                return;
            }

            var @event = rpcDescriptor.ParametersTypes
                .Select(type => _serializer.Deserialize(type, callContext.Payload))
                .ToArray();

            await rpcDescriptor
                .HubRpc(
                    hubContext: new HubContext(
                        peerId: callContext.Peer.PeerId,
                        hubId: callContext.HubId,
                        rpcId: callContext.RpcId,
                        roomId: callContext.RoomId,
                        udpMode: callContext.UdpMode),
                    hubClients: _hubClients,
                    roomManager: _roomManager,
                    ctorArguments: _ctorArgumentsResolver
                        .GetInstances(rpcDescriptor.CtorArguments)
                        .ToArray(),
                    methodArguments: @event)
                .ConfigureAwait(false);
        }
    }
}