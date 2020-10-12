using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class ArraySegmentDecoder<E> : SequenceDecoder<ArraySegment<E>>
    {
        private readonly SpanLikeAdapter<E> adapter;

        public ArraySegmentDecoder(Converter<E> converter)
        {
            Debug.Assert(converter is not null);
            Debug.Assert(converter.Length >= 0);
            this.adapter = SpanLikeAdapterHelper.Create(converter);
        }

        public override ArraySegment<E> Decode(ReadOnlySpan<byte> span)
        {
            var data = this.adapter.Decode(span);
            Debug.Assert((uint)data.Length <= (uint)data.Memory.Length);
            return new ArraySegment<E>(data.Memory, 0, data.Length);
        }
    }
}
