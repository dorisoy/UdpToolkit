namespace UdpToolkit.Benchmarks.Sandbox.Serialization
{
    using System;
    using System.Buffers;
    using BenchmarkDotNet.Attributes;
    using ProtoBuf;
    using UdpToolkit.Network.Contracts.Pooling;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class NetProtobufSerializationBenchmark
    {
#pragma warning disable SA1401
        [Params(100, 1000, 10_000)]
        public int Repeats;
#pragma warning restore SA1401

        private ConcurrentPool<GuidData> _poolWithGuidData;
        private ConcurrentPool<StringData> _poolWithStringData;
        private ArrayBufferWriter<byte> _bufferWriter;

        [IterationSetup(Targets = new[] { nameof(Serialize_GuidData), nameof(Deserialize_GuidData) })]
        public void GuidDataSetup()
        {
            _poolWithGuidData = new ConcurrentPool<GuidData>(
                factory: (pool) =>
                {
                    var data = new GuidData(pool);
                    data.Set(
                        id: Guid.NewGuid(),
                        name: Guid.NewGuid(),
                        address: Guid.NewGuid());
                    return data;
                },
                initSize: Repeats);

            var bufferWriter = new ArrayBufferWriter<byte>(1500);
            using (var pooledObject = _poolWithGuidData.GetOrCreate())
            {
                ProtoBuf.Serializer.Serialize(bufferWriter, pooledObject);
            }

            _bufferWriter = bufferWriter;
        }

        [IterationSetup(Targets = new[] { nameof(Serialize_StringData), nameof(Deserialize_StringData) })]
        public void StringDataSetup()
        {
            _poolWithStringData = new ConcurrentPool<StringData>(
                factory: (pool) =>
                {
                    var data = new StringData(pool);
                    data.Set(
                        id: Guid.NewGuid().ToString(),
                        name: Guid.NewGuid().ToString(),
                        address: Guid.NewGuid().ToString());
                    return data;
                },
                initSize: Repeats);

            var bufferWriter = new ArrayBufferWriter<byte>(1500);
            using (var pooledObject = _poolWithStringData.GetOrCreate())
            {
                ProtoBuf.Serializer.Serialize(bufferWriter, pooledObject);
            }

            _bufferWriter = bufferWriter;
        }

        [Benchmark]
        public void Serialize_GuidData()
        {
            for (int i = 0; i < Repeats; i++)
            {
                using (var guidData = _poolWithGuidData.GetOrCreate())
                {
                    guidData.Set(
                        id: Guid.NewGuid(),
                        name: Guid.NewGuid(),
                        address: Guid.NewGuid());

                    ProtoBuf.Serializer.Serialize(guidData.BufferWriter, guidData);

                    if (guidData.BufferWriter.WrittenCount == 0)
                    {
                        throw new Exception("WrittenCount is zero!");
                    }
                }
            }
        }

        [Benchmark]
        public void Deserialize_GuidData()
        {
            for (int i = 0; i < Repeats; i++)
            {
                using (var guidData = _poolWithGuidData.GetOrCreate())
                {
                    ProtoBuf.Serializer.Deserialize<GuidData>(_bufferWriter.WrittenSpan, guidData);
                }
            }
        }

        [Benchmark]
        public void Serialize_StringData()
        {
            for (int i = 0; i < Repeats; i++)
            {
                using (var pooledObject = _poolWithStringData.GetOrCreate())
                {
                    pooledObject.Set(
                        id: "id",
                        name: "name",
                        address: "address");

                    ProtoBuf.Serializer.Serialize(pooledObject.BufferWriter, pooledObject);

                    if (pooledObject.BufferWriter.WrittenCount == 0)
                    {
                        throw new Exception("WrittenCount is zero!");
                    }
                }
            }
        }

        [Benchmark]
        public void Deserialize_StringData()
        {
            for (int i = 0; i < Repeats; i++)
            {
                using (var pooledObject = _poolWithStringData.GetOrCreate())
                {
                    ProtoBuf.Serializer.Deserialize<StringData>(_bufferWriter.WrittenSpan, pooledObject);
                }
            }
        }

        [ProtoContract]
        public class GuidData : IDisposable
        {
            private readonly ConcurrentPool<GuidData> _pool;

            public GuidData(ConcurrentPool<GuidData> pool)
            {
                _pool = pool;
                BufferWriter = new ArrayBufferWriter<byte>(1024);
            }

            [ProtoMember(1)]
            public Guid Id { get; private set; }

            [ProtoMember(2)]
            public Guid Name { get; private set; }

            [ProtoMember(3)]
            public Guid Address { get; private set; }

            [ProtoIgnore]
            public ArrayBufferWriter<byte> BufferWriter { get; }

            public void Set(
                Guid id,
                Guid name,
                Guid address)
            {
                Id = id;
                Name = name;
                Address = address;
            }

            public void Dispose()
            {
                BufferWriter.Clear();
                _pool.Return(this);
            }
        }

        [ProtoContract]
        public class StringData : IDisposable
        {
            private readonly ConcurrentPool<StringData> _pool;

            public StringData(
                ConcurrentPool<StringData> pool)
            {
                _pool = pool;
                BufferWriter = new ArrayBufferWriter<byte>(1024);
            }

            [ProtoMember(1)]
            public string Id { get; private set; }

            [ProtoMember(2)]
            public string Name { get; private set; }

            [ProtoMember(3)]
            public string Address { get; private set; }

            [ProtoIgnore]
            public ArrayBufferWriter<byte> BufferWriter { get; }

            public void Set(
                string id,
                string name,
                string address)
            {
                Id = id;
                Name = name;
                Address = address;
            }

            public void Dispose()
            {
                BufferWriter.Clear();
                _pool.Return(this);
            }
        }
    }
}