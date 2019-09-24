using Microsoft.FSharp.Collections;
using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters;
using System;

namespace Mikodev.Binary.Creators.Others
{
    internal sealed class FSharpListConverter<T> : VariableConverter<FSharpList<T>>
    {
        private readonly Converter<T> converter;

        private readonly Adapter<T> adapter;

        public FSharpListConverter(Converter<T> converter)
        {
            this.converter = converter;
            adapter = (Adapter<T>)AdapterHelper.Create(converter);
        }

        public override void ToBytes(ref Allocator allocator, FSharpList<T> item)
        {
            if (item == null)
                return;
            var list = item;
            var tail = list.TailOrNull;
            while (tail != null)
            {
                var head = list.HeadOrDefault;
                converter.ToBytesWithMark(ref allocator, head);
                list = tail;
                tail = list.TailOrNull;
            }
        }

        public override FSharpList<T> ToValue(in ReadOnlySpan<byte> span)
        {
            // recursive call may cause stackoverflow, so ...
            var source = adapter.ToList(in span);
            var length = source.Count;
            var result = ListModule.Empty<T>();
            for (var i = length - 1; i >= 0; i--)
                result = FSharpList<T>.Cons(source[i], result);
            return result;
        }
    }
}
