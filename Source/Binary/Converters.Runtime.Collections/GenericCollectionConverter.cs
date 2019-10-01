using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Components;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericCollectionConverter<T, E> : VariableConverter<T> where T : IEnumerable<E>
    {
        private readonly CollectionConverter<T, E> converter;

        private readonly ToCollection<T, E> constructor;

        public GenericCollectionConverter(ToCollection<T, E> constructor, Converter<E> converter, bool reverse)
        {
            this.constructor = constructor;
            this.converter = new CollectionConverter<T, E>(converter, reverse);
        }

        public override void ToBytes(ref Allocator allocator, T item)
        {
            converter.Of(ref allocator, item);
        }

        public override T ToValue(in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var source = converter.To(in span);
            return constructor.Invoke(source);
        }
    }
}
