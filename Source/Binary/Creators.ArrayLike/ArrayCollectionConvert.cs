using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayCollectionConvert<T> : CollectionConvert<T[], T>
    {
        public override ReadOnlySpan<T> Of(T[] item) => item;

        public override T[] To(in ArraySegment<T> item) => item.Array is { } array && array.Length == item.Count ? array : item.AsSpan().ToArray();
    }
}
