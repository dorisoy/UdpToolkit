namespace UdpToolkit.Benchmarks.Sandbox.Pooling
{
    using System;
    using System.Buffers;

    /// <summary>
    /// Non allocating memory pool.
    /// </summary>
    /// <typeparam name="T">Type of polled object.</typeparam>
    /// <remarks>https://gist.github.com/GrabYourPitchforks/8efb15abbd90bc5b128f64981766e834#creating-custom-pools .</remarks>
    public abstract class NonAllocatingPool<T> : MemoryPool<T>
    {
        public static readonly ArrayPool<T> SharedPool = ArrayPool<T>.Create(4096, 20_000);

        public static new NonAllocatingPool<T>.Impl Shared { get; } = new NonAllocatingPool<T>.Impl();

        public override int MaxBufferSize => 1024;

        public override IMemoryOwner<T> Rent(int minBufferSize) => RentCore(minBufferSize);

        protected override void Dispose(bool disposing)
        {
        }

        private MemoryOwner<T> RentCore(int minBufferSize) => new MemoryOwner<T>(minBufferSize);

        public sealed class Impl : NonAllocatingPool<T>
        {
            // Typed to return the actual type rather than the
            // interface to avoid boxing, like how List<T>.GetEnumerator()
            // returns List<T>.Enumerator instead of IEnumerator<T>.
            public new MemoryOwner<T> Rent(int minBufferSize) => RentCore(minBufferSize);
        }
    }
}