using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class EnumerableAdaptedConverter<T, E> : CollectionAdaptedConverter<T, T, MemoryItem<E>, E> where T : IEnumerable<E>
    {
        public EnumerableAdaptedConverter(CollectionBuilder<T, T, MemoryItem<E>> builder, Converter<E> converter)
            : base(new EnumerableAdapter<T, E>(converter), builder, converter.Length)
        { }
    }
}
