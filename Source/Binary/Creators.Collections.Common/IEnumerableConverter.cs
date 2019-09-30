using Mikodev.Binary.Converters.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections.Common
{
    internal sealed class IEnumerableConverter<R, E> : CollectionConverter<R, E> where R : IEnumerable<E>
    {
        public IEnumerableConverter(Converter<E> converter) : base(converter, reverse: false) { }

        public override R ToValue(in ReadOnlySpan<byte> span) => (R)(object)To(in span);
    }
}
