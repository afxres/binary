using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ArrayBuilder<T> : SpanLikeBuilder<T[], T>
    {
        public override ReadOnlySpan<T> Handle(T[] item) => item;

        public override T[] Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var result = adapter.Decode(span);
            Debug.Assert((uint)result.Length <= (uint)result.Memory.Length);
            var buffer = result.Memory;
            var length = result.Length;
            if (buffer.Length == length)
                return buffer;
            var target = new T[length];
            Array.Copy(buffer, 0, target, 0, length);
            return target;
        }
    }
}
