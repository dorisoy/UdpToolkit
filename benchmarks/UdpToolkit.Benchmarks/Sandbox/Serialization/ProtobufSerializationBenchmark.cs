namespace UdpToolkit.Benchmarks.Sandbox.Serialization
{
    using System;
    using System.Buffers;
    using BenchmarkDotNet.Attributes;
    using Serializers;
    using UdpToolkit.Messages;
    using UdpToolkit.Network.Contracts.Pooling;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class ProtobufSerializationBenchmark
    {
#pragma warning disable SA1401
        [Params(100, 1000, 5000)]
        public int Repeats;
#pragma warning restore SA1401

        private ProtobufSerializer _protobufSerializer;
        private ArrayBufferWriter<byte> _bufferWriter;
        private ConcurrentPool<PersonWrapper> _pool;

        [IterationSetup]
        public void IterationSetup()
        {
            _pool = new ConcurrentPool<PersonWrapper>(
                factory: (pool) => new PersonWrapper(new Person { FirstName = "FirstName", LastName = "LastName", Id = "Id" }, pool),
                initSize: Repeats);

            _bufferWriter = new ArrayBufferWriter<byte>(1500);

            var person = new Person { FirstName = "FirstName", LastName = "LastName", Id = "Id" };
            _protobufSerializer = new Serializers.ProtobufSerializer();
            _protobufSerializer.Serialize(_bufferWriter, person);
        }

        [Benchmark]
        public void ProtobufSerializer_Serialize()
        {
            for (int i = 0; i < Repeats; i++)
            {
                using (var personWrapper = _pool.GetOrCreate())
                {
                    personWrapper.Set("firstName", "lastName", "id");

                    _protobufSerializer.Serialize(personWrapper.BufferWriter, personWrapper.Person);

                    if (personWrapper.BufferWriter.WrittenCount == 0)
                    {
                        throw new Exception("WrittenCount is zero!");
                    }
                }
            }
        }

        [Benchmark]
        public void ProtobufSerializer_Deserialize()
        {
            for (int i = 0; i < Repeats; i++)
            {
                using (var personWrapper = _pool.GetOrCreate())
                {
                    _protobufSerializer.Deserialize(_bufferWriter.WrittenMemory.Span, personWrapper.Person);
                }
            }
        }

        private class PersonWrapper : IDisposable
        {
            private readonly ConcurrentPool<PersonWrapper> _pool;

            public PersonWrapper(
                Person person,
                ConcurrentPool<PersonWrapper> pool)
            {
                Person = person;
                _pool = pool;
                BufferWriter = new ArrayBufferWriter<byte>();
            }

            public Person Person { get; }

            public ArrayBufferWriter<byte> BufferWriter { get; }

            public void Set(
                string firstName,
                string lastName,
                string id)
            {
                Person.LastName = lastName;
                Person.FirstName = firstName;
                Person.Id = id;
            }

            public void Dispose()
            {
                BufferWriter.Clear();
                _pool.Return(this);
            }
        }
    }
}