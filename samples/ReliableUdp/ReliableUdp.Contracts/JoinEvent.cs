namespace ReliableUdp.Contracts
{
    using System;
    using ProtoBuf;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework;

    [UdpEvent]
    [ProtoContract]
    public sealed class JoinEvent : IDisposable
    {
        [Obsolete("Serialization only")]
        public JoinEvent()
        {
        }

        public JoinEvent(
            Guid groupId,
            string nickname)
        {
            GroupId = groupId;
            Nickname = nickname;
        }

        [ProtoMember(1)]
        public Guid GroupId { get; private set;  }

        [ProtoMember(2)]
        public string Nickname { get; private set; }

        public JoinEvent Set(
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
            ObjectsPool<JoinEvent>.Return(this);
        }
    }
}