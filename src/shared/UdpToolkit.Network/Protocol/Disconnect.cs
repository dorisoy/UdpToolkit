namespace UdpToolkit.Network.Protocol
{
    using System;
    using System.IO;

    public sealed class Disconnect : ProtocolEvent<Disconnect>
    {
        [Obsolete("Deserialization only")]
        public Disconnect()
        {
        }

        public Disconnect(Guid connectionId)
        {
            ConnectionId = connectionId;
        }

        public Guid ConnectionId { get; }

        protected override byte[] SerializeInternal(Disconnect @event)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(buffer: @event.ConnectionId.ToByteArray());

                bw.Flush();
                return ms.ToArray();
            }
        }

        protected override Disconnect DeserializeInternal(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                return new Disconnect(
                    connectionId: new Guid(reader.ReadBytes(16)));
            }
        }
    }
}