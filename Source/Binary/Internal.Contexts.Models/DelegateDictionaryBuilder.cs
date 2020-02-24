using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal delegate T ToDictionary<out T, K, V>(IDictionary<K, V> dictionary);

    internal sealed class DelegateDictionaryBuilder<T, K, V> : CollectionBuilder<T, T, Dictionary<K, V>>
    {
        private readonly ToDictionary<T, K, V> constructor;

        public DelegateDictionaryBuilder(ToDictionary<T, K, V> constructor) => this.constructor = constructor;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<Dictionary<K, V>> adapter, ReadOnlySpan<byte> span)
        {
            if (constructor is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var data = adapter.To(span);
            return constructor.Invoke(data);
        }
    }
}
