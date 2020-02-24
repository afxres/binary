namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class CollectionAdaptedConverter<T, R, E> : CollectionAdaptedConverter<T, T, R, E>
    {
        public CollectionAdaptedConverter(CollectionAdapter<T, R> adapter, CollectionBuilder<T, T, R> builder, int itemLength) : base(adapter, builder, itemLength) { }
    }
}
