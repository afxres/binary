using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IEnumerableBuilder<T, E> : EnumerableBuilder<T, E> where T : IEnumerable<E>
    {
        public override T To(CollectionAdapter<MemoryItem<E>> adapter, ReadOnlySpan<byte> span) => (T)(object)adapter.To(span).AsArraySegment();
    }
}
