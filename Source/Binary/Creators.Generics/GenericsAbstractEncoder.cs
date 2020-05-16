namespace Mikodev.Binary.Creators.Generics
{
    internal abstract class GenericsAbstractEncoder<T>
    {
        public abstract void EncodeWithLengthPrefix(ref Allocator allocator, T item);
    }
}
