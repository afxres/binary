using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class MemoryCollectionConvert<T> : CollectionConvert<Memory<T>, ReadOnlyMemory<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(Memory<T> item) => item;

        public override Memory<T> To(in ArraySegment<T> item) => item;
    }
}
