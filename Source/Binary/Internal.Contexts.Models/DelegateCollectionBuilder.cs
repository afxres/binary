using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Internal.Contexts.Models
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

        public override T To(CollectionAdapter<MemoryItem<E>> adapter, ReadOnlySpan<byte> span)
        {
            if (constructor is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var data = adapter.To(span).AsArraySegment();
            if (reverse)
                MemoryExtensions.Reverse(data.AsSpan());
            return constructor.Invoke(data);
        }
    }
}
