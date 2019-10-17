using Mikodev.Binary.CollectionAdapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ListFallbackCollectionConvert<T> : CollectionConvert<List<T>, ReadOnlyMemory<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(List<T> item) => item?.ToArray();

        public override List<T> To(in ArraySegment<T> item) => new List<T>(item);
    }
}
