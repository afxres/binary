using Mikodev.Binary.Converters.Abstractions;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericCollectionConverter<R, E> : CollectionConverter<R, E> where R : IEnumerable<E>
    {
        private readonly ToCollection<R, E> constructor;

        public GenericCollectionConverter(Converter<E> converter, ToCollection<R, E> constructor, bool reverse) : base(converter, reverse)
        {
            this.constructor = constructor;
        }

        public override R ToValue(in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<R>();
            var source = To(in span);
            return constructor.Invoke(source);
        }
    }
}
