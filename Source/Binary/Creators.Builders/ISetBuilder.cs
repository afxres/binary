using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class ISetBuilder<T, E> : EnumerableBuilder<T, E> where T : ISet<E>
    {
        public override T To(CollectionAdapter<MemoryItem<E>> adapter, ReadOnlySpan<byte> span) => (T)(object)new HashSet<E>(adapter.To(span).AsArraySegment());
    }
}
