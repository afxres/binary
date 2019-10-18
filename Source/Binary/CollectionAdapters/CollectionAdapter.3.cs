namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionAdapter<U, R, E> : CollectionAdapter<R, E>
    {
        public abstract void Of(ref Allocator allocator, U item);
    }
}
