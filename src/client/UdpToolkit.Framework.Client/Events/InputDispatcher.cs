namespace UdpToolkit.Framework.Client.Events
{
    using System.Collections.Concurrent;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Client.Core;
    using UdpToolkit.Network.Packets;

    public sealed class InputDispatcher
    {
        private static readonly ConcurrentDictionary<RpcDescriptorId, IEventConsumer> InputQueues =
            new ConcurrentDictionary<RpcDescriptorId, IEventConsumer>();

        public void Dispatch(NetworkPacket networkPacket)
        {
            var rpcDescriptorId = new RpcDescriptorId(
                hubId: networkPacket.FrameworkHeader.HubId,
                rpcId: networkPacket.FrameworkHeader.RpcId);

            if (!InputQueues.TryGetValue(key: rpcDescriptorId, value: out var queue))
            {
                return;
            }

            queue.Enqueue(payload: networkPacket.Payload);
        }

        public void AddEventConsumer(IEventConsumer eventConsumer)
        {
            InputQueues.AddOrUpdate(
                key: eventConsumer.RpcDescriptorId,
                addValueFactory: (key) => eventConsumer,
                updateValueFactory: (key, value) => eventConsumer);
        }
    }
}
