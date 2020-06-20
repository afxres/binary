using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Sequence.Adapters
{
    internal sealed class LinkedListAdapter<E> : SequenceAdapter<LinkedList<E>, LinkedList<E>>
    {
        private readonly Converter<E> converter;

        public LinkedListAdapter(Converter<E> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, LinkedList<E> item)
        {
            if (item is null)
                return;
            for (var i = item.First; i != null; i = i.Next)
                converter.EncodeAuto(ref allocator, i.Value);
        }

        public override LinkedList<E> Decode(ReadOnlySpan<byte> span)
        {
            var body = span;
            var list = new LinkedList<E>();
            while (!body.IsEmpty)
                _ = list.AddLast(converter.DecodeAuto(ref body));
            return list;
        }
    }
}
