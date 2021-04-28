using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ListBuilder<T> : SpanLikeBuilder<List<T>, T>
    {
        private readonly Func<T[], int, List<T>> decode;

        public ListBuilder(Func<T[], int, List<T>> decode)
        {
            this.decode = decode;
        }

        public override ReadOnlySpan<T> Handle(List<T> item)
        {
            return CollectionsMarshal.AsSpan(item);
        }

        public override List<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var (buffer, length) = adapter.Decode(span);
            Debug.Assert((uint)length <= (uint)buffer.Length);
            return this.decode.Invoke(buffer, length);
        }
    }
}
