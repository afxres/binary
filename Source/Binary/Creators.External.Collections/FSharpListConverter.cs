using Microsoft.FSharp.Collections;
using Mikodev.Binary.CollectionAdapters;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.External.Collections
{
    internal sealed class FSharpListConverter<T> : Converter<FSharpList<T>>
    {
        private readonly Converter<T> converter;

        private readonly CollectionAdapter<ReadOnlyMemory<T>, ArraySegment<T>, T> adapter;

        public FSharpListConverter(Converter<T> converter)
        {
            this.converter = converter;
            adapter = CollectionAdapterHelper.Create(converter);
            Debug.Assert(adapter != null);
            Debug.Assert(converter != null);
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
            var origin = adapter.To(in span);
            var source = origin.Array;
            var length = origin.Count;
            var result = ListModule.Empty<T>();
            for (var i = length - 1; i >= 0; i--)
                result = FSharpList<T>.Cons(source[i], result);
            return result;
        }
    }
}
