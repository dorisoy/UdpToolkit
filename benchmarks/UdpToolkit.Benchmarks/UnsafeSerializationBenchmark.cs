namespace UdpToolkit.Benchmarks
{
    using System;
    using System.Buffers;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Serialization;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public unsafe class UnsafeSerializationBenchmark
    {
#pragma warning disable SA1401
        [Params(100, 1000, 5000)]
        public int Repeats;
#pragma warning restore SA1401

        private static readonly int NetworkHeaderSize = sizeof(NetworkHeader);

        [Benchmark]
        public void Serialization()
        {
            for (int i = 0; i < Repeats; i++)
            {
                var array = ArrayPool<byte>.Shared.Rent(NetworkHeaderSize);
                var span = array.AsSpan();
                _ = UnsafeSerialization.Deserialize<NetworkHeader>(span.Slice(NetworkHeaderSize));
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        [Benchmark]
        public void Deserialization()
        {
            for (int i = 0; i < Repeats; i++)
            {
                var networkHeader = new NetworkHeader(
                    channelId: 1,
                    id: 1,
                    acks: 1,
                    connectionId: Guid.NewGuid(),
                    packetType: PacketType.Connect,
                    dataType: 1);

                var array = ArrayPool<byte>.Shared.Rent(NetworkHeaderSize);
                UnsafeSerialization.Serialize(array, networkHeader);
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }
}