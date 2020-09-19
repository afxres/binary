using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Adapters
{
    internal sealed class SetAdapter<T, E> : SequenceAdapter<T, HashSet<E>> where T : ISet<E>
    {
        private readonly Converter<E> converter;

        private readonly SpanLikeAdapter<E> adapter;

        public SetAdapter(Converter<E> converter)
        {
            this.converter = converter;
            this.adapter = SpanLikeAdapterHelper.Create(converter);
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            const int Limits = 8;
            if (item is null)
                return;
            if (item is HashSet<E> { Count: var count } set && count < Limits)
                foreach (var i in set)
                    converter.EncodeAuto(ref allocator, i);
            else
                adapter.Encode(ref allocator, SequenceMethods.GetContents(item));
        }

        public override HashSet<E> Decode(ReadOnlySpan<byte> span)
        {
            var body = span;
            var item = new HashSet<E>();
            while (body.IsEmpty is false)
                _ = item.Add(converter.DecodeAuto(ref body));
            return item;
        }
    }
}
