using Microsoft.FSharp.Collections;
using System;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class FSharpListConverter<T> : Converter<FSharpList<T>>
    {
        private readonly Converter<T> converter;

        private readonly Converter<Memory<T>> memoryConverter;

        public FSharpListConverter(Converter<T> converter, Converter<Memory<T>> memoryConverter)
        {
            this.converter = converter;
            this.memoryConverter = memoryConverter;
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
            var data = memoryConverter.Decode(in span).Span;
            var list = ListModule.Empty<T>();
            for (var i = data.Length - 1; i >= 0; i--)
                list = FSharpList<T>.Cons(data[i], list);
            return list;
        }
    }
}
