using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ReadOnlyMemoryCollectionConvert<T> : CollectionConvert<ReadOnlyMemory<T>, ReadOnlyMemory<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(ReadOnlyMemory<T> item) => item;

        public override ReadOnlyMemory<T> To(in ArraySegment<T> item) => item;
    }
}
