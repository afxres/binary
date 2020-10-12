namespace Mikodev.Binary.Internal.Sequence
{
    internal abstract class SequenceEncoder<T>
    {
        public abstract void Encode(ref Allocator allocator, T item);
    }
}
