using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ArrayBuilder<T> : ArrayLikeBuilder<T[], T>
    {
        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(T[] item) => item;

        public override T[] To(CollectionAdapter<MemoryItem<T>> adapter, ReadOnlySpan<byte> span)
        {
            var data = adapter.To(span);
            Debug.Assert(data.Buffer != null && data.Length >= 0 && data.Length <= data.Buffer.Length);
            var buffer = data.Buffer;
            var length = data.Length;
            if (buffer.Length == length)
                return buffer;
            return new ReadOnlySpan<T>(buffer, 0, length).ToArray();
        }
    }
}
