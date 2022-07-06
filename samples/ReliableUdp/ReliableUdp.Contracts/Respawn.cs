namespace ReliableUdp.Contracts
{
    using System;
    using ProtoBuf;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework;

    [UdpEvent]
    [ProtoContract]
    public sealed class Respawn : IDisposable
    {
        [Obsolete("Serialization only")]
        public Respawn()
        {
        }

        public Respawn(
            string nickname,
            Guid groupId)
        {
            Nickname = nickname;
            GroupId = groupId;
        }

        [ProtoMember(1)]
        public Guid GroupId { get; private set; }

        [ProtoMember(2)]
        public string Nickname { get; private set; }

        public void Dispose()
        {
            Console.WriteLine($"{this.GetType().Name} returned to pool.");
            ObjectsPool<Respawn>.Return(this);
        }

        public Respawn Setup(
            string nickname,
            Guid groupId)
        {
            GroupId = groupId;
            Nickname = nickname;
            return this;
        }
    }
}