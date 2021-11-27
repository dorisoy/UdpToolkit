namespace UdpToolkit.Benchmarks.Sandbox.Serialization
{
    using System;
    using System.Buffers;
    using BenchmarkDotNet.Attributes;
    using MessagePack;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Serialization;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class MessagePackSerializationBenchmark
    {
#pragma warning disable SA1401
        [Params(100, 1000, 5000)]
        public int Repeats;
#pragma warning restore SA1401

        private ISerializer _serializer;
        private ReadOnlyMemory<byte> _readOnlyMemory;
        private ConcurrentPool<Ping> _pool;

        [IterationSetup]
        public void Setup()
        {
            _serializer = new Serializers.MessagePackSerializer();
            var buffer = new ArrayBufferWriter<byte>();
            var pool = new ConcurrentPool<Ping>((p) => new Ping(p), Repeats);
            _serializer.Serialize(buffer, new Ping(pool));
            _readOnlyMemory = buffer.WrittenMemory;
            _pool = pool;
        }

        [Benchmark]
        public void Serialization()
        {
            for (int i = 0; i < Repeats; i++)
            {
                using (var ping = _pool.GetOrCreate())
                {
                    _serializer.Serialize(ping.BufferWriter, ping);
                }
            }
        }

        [Benchmark]
        public void Deserialization()
        {
            for (int i = 0; i < Repeats; i++)
            {
                using (var ping = _pool.GetOrCreate())
                {
                    _ = _serializer.Deserialize<Ping>(_readOnlyMemory.Span, ping);
                }
            }
        }

        [MessagePackObject]
        public class Ping : IDisposable
        {
            private readonly ConcurrentPool<Ping> _pool;

            [Obsolete("Deserialization only")]
            public Ping()
            {
            }

            public Ping(ConcurrentPool<Ping> pool)
            {
                _pool = pool;
                Id = Guid.NewGuid();
                BufferWriter = new ArrayBufferWriter<byte>();
            }

            [Key(0)]
            public Guid Id { get; }

            [IgnoreMember]
            public ArrayBufferWriter<byte> BufferWriter { get; }

            public void Dispose()
            {
                BufferWriter.Clear();
                _pool.Return(this);
            }
        }
    }
}