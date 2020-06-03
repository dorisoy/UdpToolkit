namespace UdpToolkit.Framework.Server.Peers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;

    public class PeerProxy : IPeerProxy
    {
        private readonly IEnumerable<Peer> _peers;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly ISerializer _serializer;

        public PeerProxy(
            IEnumerable<Peer> peers,
            IAsyncQueue<NetworkPacket> outputQueue,
            ISerializer serializer)
        {
            _peers = peers;
            _outputQueue = outputQueue;
            _serializer = serializer;
        }

        public Task SendAsync<TEvent>(TEvent @event, HubContext hubContext)
        {
            var bytes = _serializer.Serialize(@event: @event);

            foreach (var peer in _peers)
            {
                var frameworkHeader = new FrameworkHeader(
                        hubId: hubContext.HubId,
                        rpcId: hubContext.RpcId,
                        roomId: hubContext.RoomId);

                _outputQueue.Produce(
                    new NetworkPacket(
                        frameworkHeader: frameworkHeader,
                        udpMode: hubContext.UdpMode.Map(),
                        payload: bytes,
                        ipEndPoint: peer.IpEndPoint));
            }

            return Task.CompletedTask;
        }
    }
}
