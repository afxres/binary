using Mikodev.Binary.Internal.Adapters;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class ArrayBuilder<T> : ArrayLikeBuilder<T[], T>
    {
        public override ReadOnlyMemory<T> Of(T[] item) => item;

        public override T[] To(CollectionAdapter<ArraySegment<T>> adapter, ReadOnlySpan<byte> span)
        {
            var data = adapter.To(span);
            Debug.Assert(data.Array != null && data.Offset == 0);
            var buffer = data.Array;
            var length = data.Count;
            if (buffer.Length == length)
                return buffer;
            var result = new T[length];
            Array.Copy(buffer, 0, result, 0, length);
            return result;
        }
    }
}
