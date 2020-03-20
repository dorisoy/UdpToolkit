using System.Collections.Concurrent;
using UdpToolkit.Core;
using UdpToolkit.Framework.Rpcs;
using UdpToolkit.Network.Packets;

namespace UdpToolkit.Framework.Events
{
    public sealed class InputDispatcher
    {
        private static readonly ConcurrentDictionary<RpcDescriptorId, IEventConsumer> InputQueues =
            new ConcurrentDictionary<RpcDescriptorId, IEventConsumer>();
        
        public void Dispatch(InputUdpPacket inputUdpPacket)
        {
            var rpcDescriptorId = new RpcDescriptorId(
                hubId: inputUdpPacket.HubId,
                rpcId: inputUdpPacket.RpcId);
            
            if (!InputQueues.TryGetValue(key: rpcDescriptorId, value: out var queue))
            {
                return;
            }
            
            queue.Enqueue(payload: inputUdpPacket.Payload);
        }

        public void AddEventConsumer(IEventConsumer eventConsumer)
        {
            InputQueues.AddOrUpdate(
                key: eventConsumer.RpcDescriptorId,
                addValueFactory: (key) => eventConsumer,
                updateValueFactory: (key,value) => eventConsumer);
        }
    }
}
