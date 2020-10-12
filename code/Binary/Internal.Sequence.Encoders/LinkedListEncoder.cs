using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Encoders
{
    internal sealed class LinkedListEncoder<E> : SequenceEncoder<LinkedList<E>>
    {
        private readonly Converter<E> converter;

        public LinkedListEncoder(Converter<E> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, LinkedList<E> item)
        {
            if (item is null)
                return;
            var converter = this.converter;
            for (var i = item.First; i is not null; i = i.Next)
                converter.EncodeAuto(ref allocator, i.Value);
        }
    }
}
