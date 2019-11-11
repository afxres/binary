using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal sealed class DelegateDictionaryBuilder<T, K, V> : DictionaryBuilder<T, K, V>
    {
        private readonly ToDictionary<T, K, V> constructor;

        public DelegateDictionaryBuilder(ToDictionary<T, K, V> constructor) => this.constructor = constructor;

        public override T To(CollectionAdapter<Dictionary<K, V>> adapter, ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var item = adapter.To(span);
            return constructor.Invoke(item);
        }
    }
}
