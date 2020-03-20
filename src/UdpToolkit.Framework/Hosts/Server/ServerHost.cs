using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdpToolkit.Core;
using UdpToolkit.Framework.Hubs;
using UdpToolkit.Framework.Rpcs;
using UdpToolkit.Network.Clients;
using UdpToolkit.Network.Packets;
using UdpToolkit.Network.Peers;
using UdpToolkit.Network.Queues;
using UdpToolkit.Network.Rudp;

namespace UdpToolkit.Framework.Hosts.Server
{
    public sealed class ServerHost : IServerHost
    {
        private readonly IAsyncQueue<InputUdpPacket> _inputQueue;
        private readonly IAsyncQueue<OutputUdpPacket> _outputQueue;

        private readonly int _processWorkers;
        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly IPeerTracker _peerTracker;
        private readonly IContainer _container;
        private readonly IRpcProvider _rpcProvider;
        private readonly ISerializer _serializer;
        
        public ServerHost(
            int processWorkers,
            IAsyncQueue<OutputUdpPacket> outputQueue,
            IAsyncQueue<InputUdpPacket> inputQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            IPeerTracker peerTracker,
            IContainer container,
            IRpcProvider rpcProvider, 
            ISerializer serializer)
        {
            _processWorkers = processWorkers;
            _outputQueue = outputQueue;
            _rpcProvider = rpcProvider;
            _serializer = serializer;
            _inputQueue = inputQueue;
            _senders = senders;
            _receivers = receivers;
            _peerTracker = peerTracker;
            _container = container;
            
            foreach (var receiver in _receivers)
            {
                receiver.UdpPacketReceived += packet =>
                {
                    Console.WriteLine($"Packet from: {packet.RemotePeer.ToString()}");
                    
                    _inputQueue.Produce(packet);
                };
            }
        }

        private async Task StartProcessingPackets()
        {
            try
            {
                await ProcessPackets();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); //TODO logging
            }
        }

        private async Task ProcessPackets()
        {
            foreach (var inputUdpPacket in _inputQueue.Consume())
            {
                var getResult = _peerTracker.TryGetPeer(scopeId: inputUdpPacket.ScopeId, peerId: inputUdpPacket.PeerId, out var peer);
                if (!getResult)
                {
                    _peerTracker.AddPeer(
                        scopeId: inputUdpPacket.ScopeId,
                        peer: new Peer(
                            id: inputUdpPacket.PeerId,
                            remotePeer: inputUdpPacket.RemotePeer,
                            reliableChannel: new ReliableChannel()));
                }

                var key = new RpcDescriptorId(hubId: inputUdpPacket.HubId, rpcId: inputUdpPacket.RpcId);
                if (!_rpcProvider.TryProvide(key, out var rpcDescriptor))
                {
                    //TODO log warning

                    continue;
                }

                if (rpcDescriptor.ParametersTypes.Count > 1)
                {
                    //TODO log warning

                    continue;
                }


                //TODO check dependencies
                //TODO check method args
                
                var @event = rpcDescriptor.ParametersTypes
                    .Select(type => _serializer.Deserialize(type, inputUdpPacket.Payload))
                    .ToArray();

                await rpcDescriptor.HubRpc(
                    hubContext: new HubContext(
                        scopeId: inputUdpPacket.ScopeId,
                        hubId: inputUdpPacket.HubId,
                        rpcId: inputUdpPacket.RpcId,
                        peerId: inputUdpPacket.PeerId),
                    serializer: _serializer,
                    peerTracker: _peerTracker,
                    eventProducer: _outputQueue,
                    ctorArguments: _container
                        .GetInstances(rpcDescriptor.CtorArguments)
                        .ToArray(),
                    methodArguments: @event);
            }
        }

        private async Task StartReceivePackets(IUdpReceiver udpReceiver)
        {
            try
            {
                await udpReceiver.StartReceiveAsync();
            }
            catch (Exception e)
            {
                //TODO logging
                Console.WriteLine(e);
            }
        }

        private async Task SendPackets(IUdpSender udpSender)
        {
            try
            {
                foreach (var outputUdpPacket in _outputQueue.Consume())
                {
                    await udpSender.Send(outputUdpPacket);
                }
            }
            catch (Exception e)
            {
                //TODO logging
                Console.WriteLine(e);
            }
        }

        public Task RunAsync()
        {
            var sending = _senders
                .Select(sender => Task.Run(() => SendPackets(sender)))
                .ToList();
            
            var receiving = _receivers
                .Select(receiver => Task.Run(() => StartReceivePackets(receiver)))
                .ToList();
            
            var processing = Enumerable.Range(0, _processWorkers)
                .Select(resenderceiver => Task.Run(StartProcessingPackets))
                .ToList();

            var tasks = receiving.Concat(sending).Concat(processing);

            return Task.WhenAll(tasks);
        }
    }
}
