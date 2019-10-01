using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections.Common
{
    internal sealed class IEnumerableConverter<T, E> : VariableConverter<T> where T : IEnumerable<E>
    {
        private readonly CollectionConverter<T, E> converter;

        public IEnumerableConverter(Converter<E> converter) => this.converter = new CollectionConverter<T, E>(converter, reverse: false);

        public override void ToBytes(ref Allocator allocator, T item) => converter.Of(ref allocator, item);

        public override T ToValue(in ReadOnlySpan<byte> span) => (T)(object)converter.To(in span);
    }
}
