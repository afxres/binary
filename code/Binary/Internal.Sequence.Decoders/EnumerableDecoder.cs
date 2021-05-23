using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class EnumerableDecoder<T, E> where T : IEnumerable<E>
    {
        private readonly SpanLikeAdapter<E> invoke;

        public EnumerableDecoder(Converter<E> converter)
        {
            Debug.Assert(converter is not null);
            Debug.Assert(converter.Length >= 0);
            this.invoke = SpanLikeAdapterHelper.Create(converter);
        }

        public T Decode(ReadOnlySpan<byte> span)
        {
            var (buffer, length) = this.invoke.Decode(span);
            Debug.Assert((uint)length <= (uint)buffer.Length);
            if (buffer.Length == length)
                return (T)(IEnumerable<E>)buffer;
            return (T)(IEnumerable<E>)NativeModule.CreateList(buffer, length);
        }
    }
}
