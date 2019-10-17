using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ReadOnlyMemoryCollectionConvert<T> : CollectionConvert<ReadOnlyMemory<T>, T>
    {
        public override ReadOnlySpan<T> Of(ReadOnlyMemory<T> item) => item.Span;

        public override ReadOnlyMemory<T> To(in ArraySegment<T> item) => item;
    }
}
