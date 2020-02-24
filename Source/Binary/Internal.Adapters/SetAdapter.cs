using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class SetAdapter<T, E> : CollectionAdapter<T, HashSet<E>> where T : ISet<E>
    {
        private readonly Converter<E> converter;

        private readonly ArrayLikeAdapter<E> adapter;

        public SetAdapter(Converter<E> converter)
        {
            this.converter = converter;
            this.adapter = ArrayLikeAdapterHelper.Create(converter);
        }

        public override int Count(T item) => item.Count;

        public override void Of(ref Allocator allocator, T item)
        {
            const int Limits = 8;
            if (item is null)
                return;
            else if (item is HashSet<E> { Count: var count } set && count < Limits)
                foreach (var i in set)
                    converter.EncodeAuto(ref allocator, i);
            else
                adapter.Of(ref allocator, new ReadOnlyMemory<E>(item.ToArray()));
        }

        public override HashSet<E> To(ReadOnlySpan<byte> span)
        {
            var body = span;
            var set = new HashSet<E>();
            while (!body.IsEmpty)
                _ = set.Add(converter.DecodeAuto(ref body));
            return set;
        }
    }
}
