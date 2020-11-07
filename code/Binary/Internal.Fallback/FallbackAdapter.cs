namespace Mikodev.Binary.Internal.Fallback
{
    internal abstract class FallbackAdapter<T>
    {
        public abstract byte[] Encode(T item);

        public abstract void EncodeWithLengthPrefix(ref Allocator allocator, T item);
    }
}
