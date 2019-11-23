using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdpToolkit.Core;
using UdpToolkit.Network;

namespace UdpToolkit.Framework
{
    public sealed class UdpPacketsProcessor : IUdpPacketsProcessor
    {
        private readonly AsyncQueue<InputUdpPacket> _inputQueue;
        private readonly ServerSettings _serverSettings;
        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly IUdpSenderProxy _udpSenderProxy;

        private readonly IPeerTracker _peerTracker;
        private readonly IContainer _container;
        private readonly IRpcProvider _rpcProvider;
        private readonly ISerializer _serializer;

        public UdpPacketsProcessor(
            AsyncQueue<InputUdpPacket> inputQueue,
            ServerSettings serverSettings,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            IUdpSenderProxy udpSenderProxy,
            IPeerTracker peerTracker,
            IContainer container,
            IRpcProvider rpcProvider, 
            ISerializer serializer)
        {
            _rpcProvider = rpcProvider;
            _serializer = serializer;
            _inputQueue = inputQueue;
            _serverSettings = serverSettings;
            _senders = senders;
            _receivers = receivers;
            _udpSenderProxy = udpSenderProxy;
            _peerTracker = peerTracker;
            _container = container;
        }

        private async Task StartProcessingPackets()
        {
            foreach (var inputUdpPacket in _inputQueue.Consume())
            {
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
                
                var request = rpcDescriptor.ParametersTypes
                    .Select(type => _serializer.Deserialize(type, inputUdpPacket.Request))
                    .ToArray();

                await rpcDescriptor.HubRpc(
                    hubContext: new HubContext(
                        scopeId: inputUdpPacket.ScopeId,
                        hubId: inputUdpPacket.HubId,
                        rpcId: inputUdpPacket.RpcId,
                        peerId: inputUdpPacket.PeerId),
                    serializer: _serializer,
                    peerTracker: _peerTracker,
                    udpSenderProxy: _udpSenderProxy,
                    ctorArguments: _container
                        .GetInstances(rpcDescriptor.CtorArguments)
                        .ToArray(),
                    methodArguments: request);
            }
        }

        private async Task StartReceivePackets(IUdpReceiver udpReceiver)
        {
            try
            {
                await udpReceiver.StartReceive();
            }
            catch (Exception e)
            {
                //TODO logging
                Console.WriteLine(e);
            }
        }

        private async Task StartSendPackets(IUdpSender udpSender)
        {
            try
            {
                await udpSender.StartSending();
            }
            catch (Exception e)
            {
                //TODO logging
                Console.WriteLine(e);
            }
        }

        public Task RunAsync()
        {
            var processing = _senders
                .Select(sender => Task.Run(() => StartSendPackets(sender)))
                .ToList();
            
            var receiving = _receivers
                .Select(receiver => Task.Run(() => StartReceivePackets(receiver)))
                .ToList();
            
            var sending = Enumerable.Range(0, _serverSettings.ProcessWorkers)
                .Select(receiver => Task.Run(StartProcessingPackets))
                .ToList();

            var tasks = receiving.Concat(sending).Concat(processing);

            return Task.WhenAll(tasks);
        }
    }
}