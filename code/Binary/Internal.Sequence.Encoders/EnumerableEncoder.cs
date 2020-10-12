using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Encoders
{
    internal sealed class EnumerableEncoder<T, E> : SequenceEncoder<T> where T : IEnumerable<E>
    {
        private readonly Converter<E> converter;

        public EnumerableEncoder(Converter<E> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item is null)
                return;
            var converter = this.converter;
            foreach (var i in item)
                converter.EncodeAuto(ref allocator, i);
        }
    }
}
