namespace UdpToolkit.Core.ProtocolEvents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public sealed class Connect : ProtocolEvent<Connect>
    {
        [Obsolete("Deserialization only")]
        public Connect()
        {
        }

        public Connect(
            Guid peerId,
            List<ClientIp> clientIps)
        {
            PeerId = peerId;
            ClientIps = clientIps;
        }

        public Guid PeerId { get; }

        public List<ClientIp> ClientIps { get; }

        protected override byte[] SerializeInternal(Connect connect)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(buffer: connect.PeerId.ToByteArray());
                foreach (var server in connect.ClientIps)
                {
                    bw.Write(server.Host);
                    bw.Write(server.Port);
                }

                bw.Flush();
                return ms.ToArray();
            }
        }

        protected override Connect DeserializeInternal(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                return new Connect(
                    peerId: new Guid(reader.ReadBytes(16)),
                    clientIps: ReadServers(reader).ToList());
            }
        }

        private IEnumerable<ClientIp> ReadServers(BinaryReader reader)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                yield return new ClientIp(
                    host: reader.ReadString(),
                    port: reader.ReadInt32());
            }
        }
    }
}