namespace UdpToolkit.Core.ProtocolEvents
{
    using System;
    using System.IO;

    public class Disconnect : ProtocolEvent<Disconnect>
    {
        [Obsolete("Deserialization only")]
        public Disconnect()
        {
        }

        public Disconnect(Guid peerId)
        {
            PeerId = peerId;
        }

        public Guid PeerId { get; }

        protected override byte[] SerializeInternal(Disconnect disconnect)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(buffer: disconnect.PeerId.ToByteArray());

                bw.Flush();
                return ms.ToArray();
            }
        }

        protected override Disconnect DeserializeInternal(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                return new Disconnect(
                    peerId: new Guid(reader.ReadBytes(16)));
            }
        }
    }
}