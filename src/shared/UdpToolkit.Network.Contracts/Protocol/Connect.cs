namespace UdpToolkit.Network.Contracts.Protocol
{
    using System;
    using System.IO;

    public sealed class Connect : ProtocolEvent<Connect>
    {
        [Obsolete("Deserialization only")]
        public Connect()
        {
        }

        public Connect(
            Guid connectionId)
        {
            ConnectionId = connectionId;
        }

        public Guid ConnectionId { get; }

        protected override byte[] SerializeInternal(Connect @event)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(buffer: @event.ConnectionId.ToByteArray());

                bw.Flush();
                return ms.ToArray();
            }
        }

        protected override Connect DeserializeInternal(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                return new Connect(
                    connectionId: new Guid(reader.ReadBytes(16)));
            }
        }
    }
}