using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Sequence.Encoders
{
    internal sealed class EnumerableEncoder<T, E> where T : IEnumerable<E>
    {
        private readonly Converter<E> converter;

        public EnumerableEncoder(Converter<E> converter) => this.converter = converter;

        public void Encode(ref Allocator allocator, T item)
        {
            Debug.Assert(item is not null);
            var converter = this.converter;
            foreach (var i in item)
                converter.EncodeAuto(ref allocator, i);
        }
    }
}
