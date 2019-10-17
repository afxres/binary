using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArraySegmentCollectionConvert<T> : CollectionConvert<ArraySegment<T>, T>
    {
        public override ReadOnlySpan<T> Of(ArraySegment<T> item) => item;

        public override ArraySegment<T> To(in ArraySegment<T> item) => item;
    }
}
