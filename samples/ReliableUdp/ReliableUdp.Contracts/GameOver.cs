namespace ReliableUdp.Contracts
{
    using System;
    using ProtoBuf;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework;

    [UdpEvent]
    [ProtoContract]
    public sealed class GameOver : IDisposable
    {
        [Obsolete("Serialization only")]
        public GameOver()
        {
        }

        public GameOver(
            Guid groupId,
            string message)
        {
            GroupId = groupId;
            Message = message;
        }

        [ProtoMember(1)]
        public string Message { get; private set;  }

        [ProtoMember(2)]
        public Guid GroupId { get; private set; }

        public GameOver Setup(
            string message,
            Guid groupId)
        {
            Message = message;
            GroupId = groupId;
            return this;
        }

        public void Dispose()
        {
            Console.WriteLine($"{this.GetType().Name} returned to pool.");
            ObjectsPool<GameOver>.Return(this);
        }
    }
}