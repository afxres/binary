using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericCollectionBuilder<T, E> : CollectionBuilder<T, T, E> where T : IEnumerable<E>
    {
        private readonly ToCollection<T, E> constructor;

        private readonly bool reverse;

        public GenericCollectionBuilder(ToCollection<T, E> constructor, bool reverse)
        {
            this.constructor = constructor;
            this.reverse = reverse;
        }

        public override int Length(T item) => item is ICollection<E> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<E> adapter, in ReadOnlySpan<byte> span)
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
