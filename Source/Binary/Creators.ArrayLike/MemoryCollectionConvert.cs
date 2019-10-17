using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class MemoryCollectionConvert<T> : CollectionConvert<Memory<T>, T>
    {
        public override ReadOnlySpan<T> Of(Memory<T> item) => item.Span;

        public override Memory<T> To(in ArraySegment<T> item) => item;
    }
}
