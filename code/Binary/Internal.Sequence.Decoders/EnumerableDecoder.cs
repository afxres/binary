using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class EnumerableDecoder<T, E> where T : IEnumerable<E>
    {
        private readonly SpanLikeAdapter<E> adapter;

        public EnumerableDecoder(Converter<E> converter)
        {
            Debug.Assert(converter is not null);
            Debug.Assert(converter.Length >= 0);
            this.adapter = SpanLikeAdapterHelper.Create(converter);
        }

        public T Decode(ReadOnlySpan<byte> span)
        {
            var data = this.adapter.Decode(span);
            Debug.Assert((uint)data.Length <= (uint)data.Memory.Length);
            var buffer = data.Memory;
            var length = data.Length;
            if (buffer.Length == length)
                return (T)(IEnumerable<E>)buffer;
            return (T)(IEnumerable<E>)new ArraySegment<E>(data.Memory, 0, data.Length);
        }
    }
}
