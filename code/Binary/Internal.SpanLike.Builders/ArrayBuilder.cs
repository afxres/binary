using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ArrayBuilder<T> : SpanLikeBuilder<T[], T>
    {
        public override ReadOnlySpan<T> Handle(T[] item) => item;

        public override T[] Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var (buffer, length) = adapter.Decode(span);
            Debug.Assert((uint)length <= (uint)buffer.Length);
            if (buffer.Length == length)
                return buffer;
            var target = new T[length];
            Array.Copy(buffer, 0, target, 0, length);
            return target;
        }
    }
}
