namespace UdpToolkit.Network.Contracts.Protocol
{
    using System;
    using System.IO;

    public sealed class ConnectToPeer : ProtocolEvent<ConnectToPeer>
    {
        [Obsolete("Deserialization only")]
        public ConnectToPeer()
        {
        }

        public ConnectToPeer(
            Guid connectionId)
        {
            ConnectionId = connectionId;
        }

        public Guid ConnectionId { get; }

        protected override byte[] SerializeInternal(ConnectToPeer @event)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(buffer: @event.ConnectionId.ToByteArray());

                bw.Flush();
                return ms.ToArray();
            }
        }

        protected override ConnectToPeer DeserializeInternal(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                return new ConnectToPeer(
                    connectionId: new Guid(reader.ReadBytes(16)));
            }
        }
    }
}