using MessagePack;

namespace UdpToolkit.Tests.Resources
{
    [MessagePackObject]
    public class Message
    {
        public Message(
            byte hubId,
            byte rpcId)
        {
            HubId = hubId;
            RpcId = rpcId;
        }
        
        [Key(0)]
        public byte HubId { get; }
        [Key(1)]
        public byte RpcId { get; }
    }
}