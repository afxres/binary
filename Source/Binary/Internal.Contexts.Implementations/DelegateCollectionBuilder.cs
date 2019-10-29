using Mikodev.Binary.Internal.Adapters;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Contexts.Implementations
{
    internal sealed class DelegateCollectionBuilder<T, E> : EnumerableBuilder<T, E>
    {
        private readonly ToCollection<T, E> constructor;

        private readonly bool reverse;

        public DelegateCollectionBuilder(ToCollection<T, E> constructor, bool reverse)
        {
            this.constructor = constructor;
            this.reverse = reverse;
        }

        public override int Count(T item) => item is ICollection<E> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<MemoryItem<E>> adapter, in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var data = adapter.To(in span).AsArraySegment();
            if (reverse)
                MemoryExtensions.Reverse((Span<E>)data);
            return constructor.Invoke(data);
        }
    }
}
