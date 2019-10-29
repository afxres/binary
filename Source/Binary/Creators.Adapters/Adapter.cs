namespace Mikodev.Binary.Creators.Adapters
{
    internal abstract class Adapter<T, E>
    {
        public abstract E Of(T item);

        public abstract T To(E item);
    }
}
