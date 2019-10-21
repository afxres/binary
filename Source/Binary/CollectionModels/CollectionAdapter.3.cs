namespace Mikodev.Binary.CollectionModels
{
    internal abstract class CollectionAdapter<U, R, E> : CollectionAdapter<R>
    {
        public abstract void Of(ref Allocator allocator, U item);
    }
}
