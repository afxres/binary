using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayCollectionConvert<T> : CollectionConvert<T[], ReadOnlyMemory<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(T[] item) => item;

        public override T[] To(in ArraySegment<T> item) => item.Array is { } array && array.Length == item.Count ? array : item.AsSpan().ToArray();
    }
}
