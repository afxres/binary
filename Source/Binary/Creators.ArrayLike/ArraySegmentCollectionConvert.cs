using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArraySegmentCollectionConvert<T> : CollectionConvert<ArraySegment<T>, ReadOnlyMemory<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(ArraySegment<T> item) => item;

        public override ArraySegment<T> To(in ArraySegment<T> item) => item;
    }
}
