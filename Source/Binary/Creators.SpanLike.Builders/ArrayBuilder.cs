using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.SpanLike.Builders
{
    internal sealed class ArrayBuilder<T> : SpanLikeBuilder<T[], T>
    {
        public override ReadOnlySpan<T> Handle(T[] item) => item;

        public override T[] Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var data = adapter.Decode(span);
            Debug.Assert((uint)data.Length <= (uint)data.Memory.Length);
            var buffer = data.Memory;
            var length = data.Length;
            if (buffer.Length == length)
                return buffer;
            var result = new T[length];
            Array.Copy(buffer, 0, result, 0, length);
            return result;
        }
    }
}
