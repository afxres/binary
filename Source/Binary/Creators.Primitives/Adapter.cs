namespace Mikodev.Binary.Creators.Primitives
{
    internal abstract class Adapter<T, E>
    {
        public abstract E Of(T item);

        public abstract T To(E item);
    }
}
