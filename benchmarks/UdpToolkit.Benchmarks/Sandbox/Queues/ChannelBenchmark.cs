namespace UdpToolkit.Benchmarks.Sandbox.Queues
{
    using System;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Network.Contracts.Packets;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class ChannelBenchmark
    {
#pragma warning disable SA1401
        [Params(100, 1000, 10000)]
        public int Repeats;
#pragma warning restore SA1401

        private readonly Channel<PendingPacket> _pendingPackets = Channel.CreateUnbounded<PendingPacket>();

        [IterationSetup(Target = nameof(ChannelWriterBenchmark))]
        public void WriterSetup()
        {
            for (int i = 0; i < Environment.ProcessorCount * 2; i++)
            {
                Task.Run(async () =>
                {
                    var packet = await _pendingPackets.Reader.ReadAsync().ConfigureAwait(false);
                });
            }
        }

        [IterationSetup(Target = nameof(ChannelReaderBenchmark))]
        public void ReaderSetup()
        {
            for (int i = 0; i < Repeats; i++)
            {
                _pendingPackets.Writer.TryWrite(new PendingPacket(default, default, default, default, default, default, default));
            }
        }

        [Benchmark]
        public async ValueTask ChannelWriterBenchmark()
        {
            for (int i = 0; i < Repeats; i++)
            {
                await _pendingPackets.Writer.WriteAsync(new PendingPacket(default, default, default, default, default, default, default)).ConfigureAwait(false);
            }
        }

        [Benchmark]
        public async ValueTask ChannelReaderBenchmark()
        {
            var count = 0;
            while (await _pendingPackets.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                count++;
                await _pendingPackets.Reader.ReadAsync().ConfigureAwait(false);
                if (count == Repeats)
                {
                    break;
                }
            }
        }
    }
}