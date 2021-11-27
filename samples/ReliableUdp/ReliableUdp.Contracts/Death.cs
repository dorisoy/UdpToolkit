namespace ReliableUdp.Contracts
{
    using System;
    using ProtoBuf;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework;

    [UdpEvent]
    [ProtoContract]
    public sealed class Death : IDisposable
    {
        [Obsolete("Serialization only")]
        public Death()
        {
        }

        public Death(
            string nickname,
            Guid groupId)
        {
            Nickname = nickname;
            GroupId = groupId;
        }

        [ProtoMember(1)]
        public string Nickname { get; private set; }

        [ProtoMember(2)]
        public Guid GroupId { get; private set; }

        public Death Set(
            Guid groupId,
            string nickname)
        {
            GroupId = groupId;
            Nickname = nickname;
            return this;
        }

        public void Dispose()
        {
            Console.WriteLine($"{this.GetType().Name} returned to pool.");
            ObjectsPool<Death>.Return(this);
        }
    }
}