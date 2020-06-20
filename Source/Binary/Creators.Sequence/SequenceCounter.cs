namespace Mikodev.Binary.Creators.Sequence
{
    internal abstract class SequenceCounter<T>
    {
        public abstract int Invoke(T item);
    }
}
