namespace UdpToolkit.Network.Protocol
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
            Guid connectionId,
            int[] inputPorts)
        {
            ConnectionId = connectionId;
            InputPorts = inputPorts;
        }

        public Guid ConnectionId { get; }

        public int[] InputPorts { get; }

        protected override byte[] SerializeInternal(Connect connect)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(buffer: connect.ConnectionId.ToByteArray());
                for (var i = 0; i < connect.InputPorts.Length; i++)
                {
                    bw.Write(connect.InputPorts[i]);
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
                    connectionId: new Guid(reader.ReadBytes(16)),
                    inputPorts: ReadPorts(reader).ToArray());
            }
        }

        private int[] ReadPorts(
            BinaryReader reader)
        {
            var list = new List<int>();
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                list.Add(reader.ReadInt32());
            }

            return list.ToArray();
        }
    }
}