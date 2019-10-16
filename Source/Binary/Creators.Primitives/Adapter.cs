namespace Mikodev.Binary.Creators.Primitives
{
    internal abstract class Adapter<T, E>
    {
        public abstract E OfValue(T item);

        public abstract T ToValue(E item);
    }
}
