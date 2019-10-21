using Microsoft.FSharp.Collections;
using System;

namespace Mikodev.Binary.Collections
{
    internal sealed class FSharpListConverter<T> : Converter<FSharpList<T>>
    {
        private readonly Converter<T> converter;

        private readonly Converter<ArraySegment<T>> arraySegmentConverter;

        public FSharpListConverter(Converter<T> converter, Converter<ArraySegment<T>> arraySegmentConverter)
        {
            this.converter = converter;
            this.arraySegmentConverter = arraySegmentConverter;
        }

        public override void Encode(ref Allocator allocator, FSharpList<T> item)
        {
            if (item == null)
                return;
            var list = item;
            var tail = list.TailOrNull;
            while (tail != null)
            {
                var head = list.HeadOrDefault;
                converter.EncodeAuto(ref allocator, head);
                list = tail;
                tail = list.TailOrNull;
            }
        }

        public override FSharpList<T> Decode(in ReadOnlySpan<byte> span)
        {
            // recursive call may cause stackoverflow, so ...
            var origin = arraySegmentConverter.Decode(in span);
            var source = origin.Array;
            var length = origin.Count;
            var result = ListModule.Empty<T>();
            for (var i = length - 1; i >= 0; i--)
                result = FSharpList<T>.Cons(source[i], result);
            return result;
        }
    }
}
