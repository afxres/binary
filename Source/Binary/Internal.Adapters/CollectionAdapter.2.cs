namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class CollectionAdapter<U, R> : CollectionAdapter<R>
    {
        public abstract int Count(U item);

        public abstract void Of(ref Allocator allocator, U item);
    }
}
