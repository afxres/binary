﻿using Mikodev.Binary.CollectionModels;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Contexts.Implementations
{
    internal sealed class DelegateDictionaryBuilder<T, K, V> : DictionaryBuilder<T, K, V>
    {
        private readonly ToDictionary<T, K, V> constructor;

        public DelegateDictionaryBuilder(ToDictionary<T, K, V> constructor) => this.constructor = constructor;

        public override int Count(T item) => item is ICollection<KeyValuePair<K, V>> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<Dictionary<K, V>> adapter, in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var item = adapter.To(in span);
            return constructor.Invoke(item);
        }
    }
}
