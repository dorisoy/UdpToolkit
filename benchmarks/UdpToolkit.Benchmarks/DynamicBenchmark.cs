#pragma warning disable
namespace UdpToolkit.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using MessagePack;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class DynamicBenchmark
    {
        private static readonly Dispatcher Dispatcher = new Dispatcher();

        [Benchmark]
        public void DispatcherBenchmark_Hash()
        {
            var request = new Request(1);
            Dispatcher.SerializeHash(request);
        }

        [Benchmark]
        public void DispatcherBenchmark_Index()
        {
            var request = new Request(1);
            Dispatcher.SerializeIndex(request);
        }

        [Benchmark]
        public void DirectSerialization()
        {
            var request = new Request(1);
            MessagePackSerializer.Serialize(request);
        }
    }

    public sealed class Dispatcher
    {
        private readonly Dictionary<Type, ISerializer> _dictionary = new Dictionary<Type, ISerializer>();
        private readonly ISerializer[] _serializers = new ISerializer[10];

        public Dispatcher()
        {
            _serializers[0] = new Serializer<Request>();
            _dictionary[typeof(Request)] = new Serializer<Request>();
        }

        public byte[] SerializeHash(object obj) => _dictionary[obj.GetType()].Serialize(obj);

        public byte[] SerializeIndex(object obj) => _serializers[0].Serialize(obj);
    }

    public class Serializer<T> : ISerializer<T>
    {
        public byte[] Serialize(object obj)
        {
            var type = (T)obj;
            return MessagePackSerializer.Serialize<T>(type);
        }
    }

    [MessagePackObject]
    public class Request
    {
        public Request(
            int value)
        {
            Value = value;
        }

        [Key(0)]
        public int Value { get; }
    }

    public interface ISerializer
    {
        byte[] Serialize(object obj);
    }

    public interface ISerializer<T> : ISerializer
    {
    }
}