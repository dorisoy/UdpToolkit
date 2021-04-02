namespace UdpToolkit.Benchmarks
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using MessagePack;
    using UdpToolkit.Benchmarks.Utils;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class GenericSerializationBenchmark
    {
        private static ArrayBufferWriter<byte> _arrayBufferWriter = new ArrayBufferWriter<byte>();

        private readonly ConcurrentPool<Context> _contextPool;
        private readonly ConcurrentPool<Context2> _context2Pool;
        private readonly ConcurrentPool<Event> _eventPool;

        public GenericSerializationBenchmark()
        {
            _contextPool = new ConcurrentPool<Context>(() => new Context(), 5000);
            _context2Pool = new ConcurrentPool<Context2>(() => new Context2(), 5000);
            _eventPool = new ConcurrentPool<Event>(() => new Event(), 5000);
        }

        [Benchmark]
        public void LambdaAllocation()
        {
            var pooledContext = _contextPool.Get();
            var pooledEvent = _eventPool.Get();

            pooledEvent.Value.Set(
                x: 1,
                y: 1);

            var val = pooledEvent.Value;
            pooledContext.Value.Set(
                serializer: () => MessagePackSerializer.Serialize(writer: _arrayBufferWriter, value: val));

            pooledContext.Value.Serializer();
            pooledEvent.Dispose();
            pooledContext.Dispose();
        }

        [Benchmark]
        public void LambdaAllocation_TryFix()
        {
            using (var pooledEvent = _eventPool.Get())
            {
                pooledEvent.Value.Set(
                    x: 1,
                    y: 1);

                MessagePackSerializer.Serialize(_arrayBufferWriter, pooledEvent.Value);
            }
        }

        [Benchmark]
        public void LambdaAllocation_TryFix_Obj()
        {
            using (var pooledEvent = _eventPool.Get())
            {
                pooledEvent.Value.Set(
                    x: 1,
                    y: 1);

                object obj = pooledEvent.Value;
                var @event = (Event)obj;

                MessagePackSerializer.Serialize(_arrayBufferWriter, @event);
            }
        }

        public class Context : IResettable
        {
            public Action Serializer { get; private set; }

            public void Set(
                Action serializer)
            {
                Serializer = serializer;
            }

            public void Reset()
            {
                Serializer = null;
            }
        }

        public class Context2 : IResettable
        {
            public void Reset()
            {
            }
        }

        [MessagePackObject]
        public class Event : IResettable
        {
            [Key(0)]
            public int X { get; private set; }

            [Key(1)]
            public int Y { get; private set; }

            public void Set(
                int x,
                int y)
            {
                X = x;
                Y = y;
            }

            public void Reset()
            {
                X = default;
                Y = default;
            }
        }
    }
}