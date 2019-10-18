namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionAdapter<U, E> : CollectionAdapter<E>
    {
        public abstract void Of(ref Allocator allocator, U item);
    }
}
