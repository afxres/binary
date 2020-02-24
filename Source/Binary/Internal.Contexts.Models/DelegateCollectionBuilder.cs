using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal delegate T ToCollection<out T, in E>(IEnumerable<E> enumerable);

    internal sealed class DelegateCollectionBuilder<T, E> : CollectionBuilder<T, T, ArraySegment<E>>
    {
        private readonly ToCollection<T, E> constructor;

        private readonly bool reverse;

        public DelegateCollectionBuilder(ToCollection<T, E> constructor)
        {
            this.constructor = constructor;
            this.reverse = typeof(T) == typeof(Stack<E>) || typeof(T) == typeof(ConcurrentStack<E>);
        }

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<ArraySegment<E>> adapter, ReadOnlySpan<byte> span)
        {
            if (constructor is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var data = adapter.To(span);
            Debug.Assert(data.Array != null && data.Offset == 0);
            if (reverse)
                data.AsSpan().Reverse();
            return constructor.Invoke(data);
        }
    }
}
