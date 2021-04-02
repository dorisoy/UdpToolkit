namespace UdpToolkit.Benchmarks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Benchmarks.Utils;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class ProducerConsumerQueueBenchmark
    {
#pragma warning disable SA1401
        [Params(100, 1000, 10000)]
        public int Jobs;
#pragma warning restore SA1401

        private static readonly ConcurrentPool<ClientOutContextInternal> ContextPool =
            new ConcurrentPool<ClientOutContextInternal>(
                () => default,
                10000);

        private static readonly ChannelQueue Channel = new ChannelQueue();
        private static readonly CCQueue BlockingCollection = new CCQueue();

        private static readonly CustomQueue<PooledObject<ClientOutContextInternal>> Custom =
            new CustomQueue<PooledObject<ClientOutContextInternal>>(threadsCount: 1);

        private static readonly AutoResetEvent AutoResetEvent = new AutoResetEvent(false);
#pragma warning disable


        [Benchmark]
        public void Channel_Benchmark()
        {
            for (var i = 0; i < 10000; i++)
            {
                var contextOut = ContextPool.Get();
                contextOut.Value.Set(
                    broadcastMode: BroadcastMode.Caller,
                    resendTimeout: TimeSpan.MaxValue,
                    createdAt: DateTimeOffset.UtcNow,
                    hookId: ProtocolHookId.Connect,
                    channelType: ChannelType.Udp,
                    packetType: PacketType.Protocol,
                    connectionId: Guid.NewGuid());
        
                Channel.Produce(contextOut);
            }
#pragma warning disable
            // Channel.Produce(null);
        
            Console.WriteLine("Wait");
            AutoResetEvent.WaitOne();
        }
        
        [Benchmark]
        public void BlockingCollection_Benchmark()
        {
            for (var i = 0; i < Jobs; i++)
            {
                var contextOut = ContextPool.Get();
                contextOut.Value.Set(
                    broadcastMode: BroadcastMode.Caller,
                    resendTimeout: TimeSpan.MaxValue,
                    createdAt: DateTimeOffset.UtcNow,
                    hookId: ProtocolHookId.Connect,
                    channelType: ChannelType.Udp,
                    packetType: PacketType.Protocol,
                    connectionId: Guid.NewGuid());
        
                BlockingCollection.Produce(contextOut);
            }
        
            BlockingCollection.Produce(null);
        
            AutoResetEvent.WaitOne();
        }

        [Benchmark]
        public void CustomQueue_Benchmark()
        {
            for (int j = 0; j < Jobs; j++)
            {
                var contextOut = ContextPool.Get();
                contextOut.Value.Set(
                    broadcastMode: BroadcastMode.Caller,
                    resendTimeout: TimeSpan.MaxValue,
                    createdAt: DateTimeOffset.UtcNow,
                    hookId: ProtocolHookId.Connect,
                    channelType: ChannelType.Udp,
                    packetType: PacketType.Protocol,
                    connectionId: Guid.NewGuid());

                Custom.Produce(contextOut);
            }

            Custom.Produce(null);

            AutoResetEvent.WaitOne();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Custom.Stop();
        }

        public struct ClientOutContextInternal : IResettable
        {
            public BroadcastMode BroadcastMode { get; private set; }

            public TimeSpan ResendTimeout { get; private set; }

            public DateTimeOffset CreatedAt { get; private set; }

            public ProtocolHookId ProtocolHookId { get; private set; }

            public ChannelType ChannelType { get; private set; }

            public PacketType PacketType { get; private set; }

            public Guid ConnectionId { get; private set; }

            public void Set(
                BroadcastMode broadcastMode,
                TimeSpan resendTimeout,
                DateTimeOffset createdAt,
                ProtocolHookId hookId,
                ChannelType channelType,
                PacketType packetType,
                Guid connectionId)
            {
                BroadcastMode = broadcastMode;
                ResendTimeout = resendTimeout;
                CreatedAt = createdAt;
                ProtocolHookId = hookId;
                ChannelType = channelType;
                PacketType = packetType;
                ConnectionId = connectionId;
            }

            public void Reset()
            {
                BroadcastMode = default;
                ResendTimeout = default;
                CreatedAt = default;
                ProtocolHookId = default;
                ChannelType = default;
                PacketType = default;
                ConnectionId = default;
            }
        }

        public sealed class CustomQueue<T>
            where T : class, IDisposable
        {
            private readonly SemaphoreSlim _semaphoreSlim;
            private readonly Queue<T> _queue;
            private readonly Thread[] _threads;
            private readonly object _locker;
            private bool _cancelled;

            public CustomQueue(int threadsCount)
            {
                _semaphoreSlim = new SemaphoreSlim(1);
                _queue = new Queue<T>();
                _locker = new object();
                _threads = new Thread[threadsCount];

                for (var i = 0; i < threadsCount; i++)
                {
                    var thread = new Thread(Run);
                    _threads[i] = thread;
                    thread.Start();
                }
            }

            public void Produce(T action)
            {
                lock (_locker)
                {
                    _queue.Enqueue(action);
                    Monitor.Pulse(_locker);
                }
            }

            public void Stop()
            {
                lock (_locker)
                {
                    _cancelled = true;
                    Monitor.Pulse(_locker);
                }
                
                foreach (var thread in _threads)
                {
                    thread.Join();
                }
            }

            private void Run()
            {
                while (!_cancelled)
                {
                    lock (_locker)
                    {
                        while (!_queue.Any())
                        {
                            Monitor.Wait(_locker);
                            if (_cancelled)
                            {
                                Console.WriteLine("Cancelled");
                                return;
                            }
                        }

                        var item = _queue.Dequeue();
                        Monitor.Pulse(_locker);

                        item?.Dispose();
                        if (item == null)
                        {
                            AutoResetEvent.Set();
                        }
                    }
                }
            }
        }
        
        internal class ChannelQueue
            {
                private readonly Channel<PooledObject<ClientOutContextInternal>> _channel;

                public ChannelQueue()
                {
                    _channel =
                        System.Threading.Channels.Channel.CreateUnbounded<PooledObject<ClientOutContextInternal>>(
                            new UnboundedChannelOptions()
                            {
                                SingleReader = true,
                            });
                    Task.Run(async () =>
                    {
                        while (await _channel.Reader.WaitToReadAsync())
                        {
                            var item = await _channel.Reader.ReadAsync();
                            item?.Dispose();
                            if (item == null)
                            {
                                AutoResetEvent.Set();
                            }
                        }
                    });
                }

                public void Produce(PooledObject<ClientOutContextInternal> context)
                {
                    _channel.Writer.TryWrite(context);
                }
            }

        internal class CCQueue
        {
            private readonly BlockingCollection<PooledObject<ClientOutContextInternal>> _queue;

            public CCQueue()
            {
                _queue = new BlockingCollection<PooledObject<ClientOutContextInternal>>();
                Task.Run(async () =>
                {
                    foreach (var item in _queue.GetConsumingEnumerable())
                    {
                        item?.Dispose();
                        if (item == null)
                        {
                            AutoResetEvent.Set();
                        }
                    }
                });
            }

            public void Produce(PooledObject<ClientOutContextInternal> context)
            {
                _queue.Add(context);
            }
        }
    }
}