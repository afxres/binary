using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class LinkedListAdapter<E> : CollectionAdapter<LinkedList<E>, LinkedList<E>>
    {
        private readonly Converter<E> converter;

        public LinkedListAdapter(Converter<E> converter) => this.converter = converter;

        public override int Count(LinkedList<E> item) => item.Count;

        public override void Of(ref Allocator allocator, LinkedList<E> item)
        {
            if (item is null)
                return;
            for (var i = item.First; i != null; i = i.Next)
                converter.EncodeAuto(ref allocator, i.Value);
        }

        public override LinkedList<E> To(ReadOnlySpan<byte> span)
        {
            var body = span;
            var list = new LinkedList<E>();
            while (!body.IsEmpty)
                _ = list.AddLast(converter.DecodeAuto(ref body));
            return list;
        }
    }
}
