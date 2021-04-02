namespace UdpToolkit.Benchmarks
{
    using System;
    using System.Buffers;
    using System.Linq;
    using System.Runtime.InteropServices;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class ProtocolSerializationBenchmark
    {
        private const int PayloadLength = 100;
        private const int DestinationLength = 119;
        private static readonly Guid ConnectionId = Guid.NewGuid();
        private static readonly byte[] Payload = Enumerable.Range(0, PayloadLength).Select(x => (byte)x).ToArray();

        [Benchmark]
        public void Span_GC_Free_Version_Buffer_Block_Copy()
        {
            var destination = ArrayPool<byte>.Shared.Rent(DestinationLength);

            var packet = new Packet(
                hookId: 255,
                channelType: ChannelType.ReliableUdp,
                packetType: PacketType.Protocol,
                connectionId: ConnectionId);

            destination.Write(packet);

            Buffer.BlockCopy(src: Payload, srcOffset: 0, dst: destination, dstOffset: 19, count: PayloadLength);

            ArrayPool<byte>.Shared.Return(destination);
        }

        [Benchmark]
        public void Span_GC_Free_Version_Array_Copy()
        {
            var destination = ArrayPool<byte>.Shared.Rent(DestinationLength);

            var packet = new Packet(
                hookId: 255,
                channelType: ChannelType.ReliableUdp,
                packetType: PacketType.Protocol,
                connectionId: ConnectionId);

            destination.Write(packet);

            Array.Copy(sourceArray: Payload, sourceIndex: 0, destinationArray: destination, destinationIndex: 19, length: PayloadLength);

            ArrayPool<byte>.Shared.Return(destination);
        }

        [Benchmark]
        public void Span_GC_Free_Version_Try_AvoidSpan_Array_Copy()
        {
            var destination = ArrayPool<byte>.Shared.Rent(DestinationLength);

            var packet = new Packet(
                hookId: 255,
                channelType: ChannelType.ReliableUdp,
                packetType: PacketType.Protocol,
                connectionId: ConnectionId);

            UnsafeExtensions.WriteWithoutSpan(ref destination, ref packet);

            Array.Copy(sourceArray: Payload, sourceIndex: 0, destinationArray: destination, destinationIndex: 19, length: PayloadLength);
            ArrayPool<byte>.Shared.Return(destination);
        }

        [Benchmark]
        public void Span_GC_Free_Version_Try_AvoidSpan_Buffer_Copy()
        {
            var destination = ArrayPool<byte>.Shared.Rent(DestinationLength);

            var packet = new Packet(
                hookId: 255,
                channelType: ChannelType.ReliableUdp,
                packetType: PacketType.Protocol,
                connectionId: ConnectionId);

            UnsafeExtensions.WriteWithoutSpan(ref destination, ref packet);

            Buffer.BlockCopy(src: Payload, srcOffset: 0, dst: destination, dstOffset: 19, count: PayloadLength);

            ArrayPool<byte>.Shared.Return(destination);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public readonly struct Packet
        {
            public Packet(
                byte hookId,
                ChannelType channelType,
                PacketType packetType,
                Guid connectionId)
            {
                HookId = hookId;
                ChannelType = channelType;
                PacketType = packetType;
                ConnectionId = connectionId;
            }

            public byte HookId { get; }

            public ChannelType ChannelType { get; }

            public PacketType PacketType { get; }

            public Guid ConnectionId { get; }
        }
    }
}