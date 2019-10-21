using Mikodev.Binary.CollectionModels;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class DelegateCollectionBuilder<T, E> : CollectionBuilder<T, T, ArraySegment<E>, E>
    {
        private readonly ToCollection<T, E> constructor;

        private readonly bool reverse;

        public DelegateCollectionBuilder(ToCollection<T, E> constructor, bool reverse)
        {
            this.constructor = constructor;
            this.reverse = reverse;
        }

        public override int Length(T item) => item is ICollection<E> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<ArraySegment<E>> adapter, in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var item = adapter.To(in span);
            if (reverse)
                MemoryExtensions.Reverse((Span<E>)item);
            return constructor.Invoke(item);
        }
    }
}
