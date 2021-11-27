namespace UdpToolkit.Framework.CodeGenerator.Contracts
{
    using System;
    using System.Buffers;

    /// <summary>
    /// Backport for net standard 2.0.
    /// </summary>
    /// <typeparam name="T">Type of buffer.</typeparam>
    public sealed class BufferWriter<T> : IBufferWriter<T>
    {
        private T[] _buffer;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferWriter{T}"/> class.
        /// </summary>
        public BufferWriter()
        {
          this._buffer = Array.Empty<T>();
          this._index = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferWriter{T}"/> class.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity for buffer.</param>
        public BufferWriter(int initialCapacity)
        {
          this._buffer = initialCapacity > 0 ? new T[initialCapacity] : throw new ArgumentException((string)null, nameof(initialCapacity));
          this._index = 0;
        }

        /// <summary>
        /// Gets written memory.
        /// </summary>
        public ReadOnlyMemory<T> WrittenMemory => (ReadOnlyMemory<T>)this._buffer.AsMemory<T>(0, this._index);

        /// <summary>
        /// Gets written span.
        /// </summary>
        public ReadOnlySpan<T> WrittenSpan => (ReadOnlySpan<T>)this._buffer.AsSpan<T>(0, this._index);

        /// <summary>
        /// Gets written count.
        /// </summary>
        public int WrittenCount => this._index;

        /// <summary>
        /// Gets buffer capacity.
        /// </summary>
        public int Capacity => this._buffer.Length;

        /// <summary>
        /// Gets free capacity.
        /// </summary>
        public int FreeCapacity => this._buffer.Length - this._index;

        /// <summary>
        /// Clear buffer.
        /// </summary>
        public void Clear()
        {
          this._buffer.AsSpan<T>(0, this._index).Clear();
          this._index = 0;
        }

        /// <inheritdoc />
        public void Advance(int count)
        {
          if (count < 0)
          {
            throw new ArgumentException((string)null, nameof(count));
          }

          if (this._index > this._buffer.Length - count)
          {
            BufferWriter<T>.ThrowInvalidOperationException_AdvancedTooFar(this._buffer.Length);
          }

          this._index += count;
        }

        /// <inheritdoc />
        public Memory<T> GetMemory(int sizeHint = 0)
        {
          this.CheckAndResizeBuffer(sizeHint);
          return this._buffer.AsMemory<T>(this._index);
        }

        /// <inheritdoc />
        public Span<T> GetSpan(int sizeHint = 0)
        {
          this.CheckAndResizeBuffer(sizeHint);
          return this._buffer.AsSpan<T>(this._index);
        }

        private static void ThrowInvalidOperationException_AdvancedTooFar(int capacity) => throw new InvalidOperationException($"BufferWriterAdvancedTooFar {(object)capacity}");

        private static void ThrowOutOfMemoryException(uint capacity) => throw new OutOfMemoryException($"BufferMaximumSizeExceeded {(object)capacity}");

        private void CheckAndResizeBuffer(int sizeHint)
        {
          if (sizeHint < 0)
          {
            throw new ArgumentException(nameof(sizeHint));
          }

          if (sizeHint == 0)
          {
            sizeHint = 1;
          }

          if (sizeHint <= this.FreeCapacity)
          {
            return;
          }

          int length = this._buffer.Length;
          int val1 = Math.Max(sizeHint, length);
          if (length == 0)
          {
            val1 = Math.Max(val1, 256);
          }

          int newSize = length + val1;
          if ((uint)newSize > (uint)int.MaxValue)
          {
            newSize = length + sizeHint;
            if ((uint)newSize > (uint)int.MaxValue)
            {
              BufferWriter<T>.ThrowOutOfMemoryException((uint)newSize);
            }
          }

          Array.Resize<T>(ref this._buffer, newSize);
        }
    }
}