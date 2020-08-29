namespace Mikodev.Binary.Internal.Sequence
{
    internal abstract class SequenceCounter<T>
    {
        public abstract int Invoke(T item);
    }
}
