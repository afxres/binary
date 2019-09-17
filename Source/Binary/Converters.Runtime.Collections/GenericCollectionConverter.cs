using Mikodev.Binary.Converters.Abstractions;
using Mikodev.Binary.Delegates;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericCollectionConverter<TCollection, T> : CollectionConverter<TCollection, T> where TCollection : IEnumerable<T>
    {
        private readonly ToCollection<TCollection, T> constructor;

        public GenericCollectionConverter(Converter<T> converter, ToCollection<TCollection, T> constructor, bool reverse) : base(converter, reverse)
        {
            this.constructor = constructor;
        }

        public override TCollection ToValue(in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<TCollection>();
            var source = GetCollection(span);
            return constructor.Invoke(source);
        }
    }
}
