namespace SimpleUdp.Contracts
{
    using System;
    using MessagePack;

    [MessagePackObject]
    public class JoinedEvent
    {
        [Obsolete(message: "For deserialization only", error: true)]
        public JoinedEvent()
        {
        }

        public JoinedEvent(
            string nickname)
        {
            Nickname = nickname;
        }

        [Key(0)]
        public string Nickname { get; }
    }
}