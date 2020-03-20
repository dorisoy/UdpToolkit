using System;
using System.Linq;
using UdpToolkit.Network.Rudp;

namespace UdpToolkit.Network.Protocol
{
    public sealed class ReliableUdpProtocol : IReliableUdpProtocol
    {
        public bool TryDeserialize(byte[] bytes, out ReliableUdpHeader header)
        {
            header = default;
            if (bytes == null || bytes.Length < Consts.ReliableUdpProtocolHeaderLength)
            {
                return false;
            }

            header = new ReliableUdpHeader(
                localNumber: BitConverter.ToUInt32(value: bytes, startIndex: 0),
                ack: BitConverter.ToUInt32(value: bytes, startIndex: 4),
                acks: BitConverter.ToUInt32(value: bytes, startIndex: 8));
            
            return true;
        }

        public byte[] Serialize(ReliableUdpHeader header)
        {
            var localNumberBytes = BitConverter.GetBytes(header.LocalNumber);
            var ackBytes = BitConverter.GetBytes(header.Ack);
            var acksBytes = BitConverter.GetBytes(header.Acks);

            return localNumberBytes
                .Concat(ackBytes)
                .Concat(acksBytes)
                .ToArray();
        }
    }
}